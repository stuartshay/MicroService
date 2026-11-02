SOLUTION := MicroService.sln
WEB_PROJECT := src/MicroService.WebApi/MicroService.WebApi.csproj
TEST_PROJECT := test/MicroService.Test/MicroService.Test.csproj
CONFIGURATION ?= Debug
DOTNET ?= dotnet
PRE_COMMIT ?= $(HOME)/.local/share/MicroService/pre-commit/bin/pre-commit
RUN_ANALYZERS ?= 0
ANALYZE_CONFIGURATION ?= Release
LPC_DIRECTORY := files/LPC_Individual_Landmark_and_Historic_Building_Database
TEST_DATA_DBF := $(LPC_DIRECTORY)/LPC_Individual_Landmark_and_Historic_District_Building_Database_20231209.dbf
TEST_DATA_ZIP := $(TEST_DATA_DBF).zip

.DEFAULT_GOAL := help

.PHONY: help setup setup-gcloud hooks check analyze restore build run test test-data sonar certificates unused

help:
	@echo "Available targets:"
	@echo "  make setup    Install .NET 10 and essential development tools"
	@echo "  make setup-gcloud  Install and verify the Google Cloud CLI"
	@echo "  make hooks    Install the pre-commit Git hook"
	@echo "  make check    Run pre-commit checks against all tracked files"
	@echo "                Set RUN_ANALYZERS=1 to also run analyzer validation"
	@echo "  make analyze  Build with Roslyn analyzers and code-style checks enabled"
	@echo "  make restore  Restore NuGet packages"
	@echo "  make build    Build the solution"
	@echo "  make run      Run the Web API locally"
	@echo "  make test     Run retention checks and .NET tests (extracts missing test .dbf)"
	@echo "  make test-data  Extract the git-ignored .dbf the tests need, if missing"
	@echo "  make sonar    Build, test, and submit analysis to SonarQube"
	@echo "  make unused   Report unused code and usings (dotnet-unused, report only)"
	@echo "  make certificates  Generate local nginx development certificates"
	@echo ""
	@echo "Options:"
	@echo "  CONFIGURATION=Debug|Release (default: Debug)"
	@echo "  RUN_ANALYZERS=0|1 (default: 0)"
	@echo "  ANALYZE_CONFIGURATION=Debug|Release (default: Release)"

setup:
	./scripts/setup.sh

setup-gcloud:
	./scripts/setup_gcloud.sh

hooks:
	$(PRE_COMMIT) install --install-hooks

check:
	$(PRE_COMMIT) run --all-files
	@if [ "$(RUN_ANALYZERS)" = "1" ]; then \
		$(MAKE) analyze ANALYZE_CONFIGURATION=$(ANALYZE_CONFIGURATION) DOTNET=$(DOTNET); \
	fi

analyze: restore
	$(DOTNET) build $(SOLUTION) --configuration $(ANALYZE_CONFIGURATION) --no-restore /p:RunAnalyzers=true /p:EnforceCodeStyleInBuild=true /p:ContinuousIntegrationBuild=true /p:CodeAnalysisTreatWarningsAsErrors=true /p:TreatWarningsAsErrors=true

restore:
	$(DOTNET) restore $(SOLUTION)

build: restore
	$(DOTNET) build $(SOLUTION) --configuration $(CONFIGURATION) --no-restore

run:
	$(DOTNET) run --project $(WEB_PROJECT) --configuration $(CONFIGURATION)

test: test-data
	python3 scripts/test_dockerhub_prune.py
	python3 scripts/test_publish_shapes.py
	python3 scripts/test_shape_smoke_check.py
	python3 scripts/test_add_historic_districts.py
	$(DOTNET) test --project $(TEST_PROJECT) --configuration $(CONFIGURATION)

# A .dbf over GitHub's 100 MB limit is committed as <name>.dbf.zip and git-ignored
# unzipped; extract it the same way CI and publish-shapes do.
test-data: $(TEST_DATA_DBF)

$(TEST_DATA_DBF): $(TEST_DATA_ZIP)
	python3 scripts/publish-shapes.py unzip

sonar:
	./scripts/sonar.sh

certificates:
	./scripts/generate-certificates.sh

# Report only: lists unused symbols (public ones included) and using directives
unused:
	@command -v dotnet-unused >/dev/null || { echo "dotnet-unused not found: dotnet tool install --global dotnetunused"; exit 1; }
	dotnet-unused $(SOLUTION) --exclude-public false
