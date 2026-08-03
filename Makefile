SOLUTION := MicroService.sln
WEB_PROJECT := src/MicroService.WebApi/MicroService.WebApi.csproj
TEST_PROJECT := test/MicroService.Test/MicroService.Test.csproj
CONFIGURATION ?= Debug
DOTNET ?= dotnet
PRE_COMMIT ?= $(HOME)/.local/share/MicroService/pre-commit/bin/pre-commit
RUN_ANALYZERS ?= 0
ANALYZE_CONFIGURATION ?= Release

.DEFAULT_GOAL := help

.PHONY: help setup hooks check analyze restore build run test sonar certificates

help:
	@echo "Available targets:"
	@echo "  make setup    Install .NET 10 and essential development tools"
	@echo "  make hooks    Install the pre-commit Git hook"
	@echo "  make check    Run pre-commit checks against all tracked files"
	@echo "                Set RUN_ANALYZERS=1 to also run analyzer validation"
	@echo "  make analyze  Build with Roslyn analyzers and code-style checks enabled"
	@echo "  make restore  Restore NuGet packages"
	@echo "  make build    Build the solution"
	@echo "  make run      Run the Web API locally"
	@echo "  make test     Run the test project"
	@echo "  make sonar    Build, test, and submit analysis to SonarQube"
	@echo "  make certificates  Generate local nginx development certificates"
	@echo ""
	@echo "Options:"
	@echo "  CONFIGURATION=Debug|Release (default: Debug)"
	@echo "  RUN_ANALYZERS=0|1 (default: 0)"
	@echo "  ANALYZE_CONFIGURATION=Debug|Release (default: Release)"

setup:
	./scripts/setup.sh

hooks:
	$(PRE_COMMIT) install --install-hooks

check:
	$(PRE_COMMIT) run --all-files
	@if [ "$(RUN_ANALYZERS)" = "1" ]; then \
		$(MAKE) analyze ANALYZE_CONFIGURATION=$(ANALYZE_CONFIGURATION) DOTNET=$(DOTNET); \
	fi

analyze: restore
	$(DOTNET) build $(SOLUTION) --configuration $(ANALYZE_CONFIGURATION) --no-restore /p:RunAnalyzers=true /p:EnforceCodeStyleInBuild=true /p:ContinuousIntegrationBuild=true

restore:
	$(DOTNET) restore $(SOLUTION)

build: restore
	$(DOTNET) build $(SOLUTION) --configuration $(CONFIGURATION) --no-restore

run:
	$(DOTNET) run --project $(WEB_PROJECT) --configuration $(CONFIGURATION)

test:
	$(DOTNET) test $(TEST_PROJECT) --configuration $(CONFIGURATION)

sonar:
	./scripts/sonar.sh

certificates:
	./scripts/generate-certificates.sh
