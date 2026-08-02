SOLUTION := MicroService.sln
WEB_PROJECT := src/MicroService.WebApi/MicroService.WebApi.csproj
TEST_PROJECT := test/MicroService.Test/MicroService.Test.csproj
CONFIGURATION ?= Debug
DOTNET ?= dotnet

.DEFAULT_GOAL := help

.PHONY: help setup restore build run test

help:
	@echo "Available targets:"
	@echo "  make setup    Install .NET 10 and essential development tools"
	@echo "  make restore  Restore NuGet packages"
	@echo "  make build    Build the solution"
	@echo "  make run      Run the Web API locally"
	@echo "  make test     Run the test project"
	@echo ""
	@echo "Options:"
	@echo "  CONFIGURATION=Debug|Release (default: Debug)"

setup:
	./scripts/setup.sh

restore:
	$(DOTNET) restore $(SOLUTION)

build: restore
	$(DOTNET) build $(SOLUTION) --configuration $(CONFIGURATION) --no-restore

run:
	$(DOTNET) run --project $(WEB_PROJECT) --configuration $(CONFIGURATION)

test:
	$(DOTNET) test $(TEST_PROJECT) --configuration $(CONFIGURATION)
