SOLUTION := MicroService.sln
WEB_PROJECT := src/MicroService.WebApi/MicroService.WebApi.csproj
TEST_PROJECT := test/MicroService.Test/MicroService.Test.csproj
CONFIGURATION ?= Debug
DOTNET ?= dotnet
PRE_COMMIT ?= $(HOME)/.local/share/MicroService/pre-commit/bin/pre-commit

.DEFAULT_GOAL := help

.PHONY: help setup hooks check restore build run test sonar

help:
	@echo "Available targets:"
	@echo "  make setup    Install .NET 10 and essential development tools"
	@echo "  make hooks    Install the pre-commit Git hook"
	@echo "  make check    Run pre-commit checks against all tracked files"
	@echo "  make restore  Restore NuGet packages"
	@echo "  make build    Build the solution"
	@echo "  make run      Run the Web API locally"
	@echo "  make test     Run the test project"
	@echo "  make sonar    Build, test, and submit analysis to SonarQube"
	@echo ""
	@echo "Options:"
	@echo "  CONFIGURATION=Debug|Release (default: Debug)"

setup:
	./scripts/setup.sh

hooks:
	$(PRE_COMMIT) install --install-hooks

check:
	$(PRE_COMMIT) run --all-files

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
