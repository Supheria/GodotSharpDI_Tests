#!/bin/bash

# Build and Test Script for GodotSharpDI Tests

set -e

echo "==================================="
echo "GodotSharpDI Test Build Script"
echo "==================================="
echo ""

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Check if dotnet is installed
if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}Error: .NET SDK is not installed${NC}"
    exit 1
fi

echo -e "${YELLOW}Step 1: Restore NuGet packages${NC}"
dotnet restore GodotSharpDI_Tests.csproj
echo ""

echo -e "${YELLOW}Step 2: Build the project${NC}"
dotnet build GodotSharpDI_Tests.csproj --configuration Debug
echo ""

echo -e "${YELLOW}Step 3: Run unit tests${NC}"
dotnet test GodotSharpDI_Tests.csproj \
    --configuration Debug \
    --filter "FullyQualifiedName~GodotSharpDI.Tests.Unit" \
    --logger "console;verbosity=detailed"
echo ""

TEST_RESULT=$?

if [ $TEST_RESULT -eq 0 ]; then
    echo -e "${GREEN}✓ All unit tests passed!${NC}"
    echo ""
    echo -e "${YELLOW}Integration tests require Godot engine.${NC}"
    echo "To run integration tests:"
    echo "1. Open this project in Godot 4.3+"
    echo "2. Build the C# project (Build → Build Project)"
    echo "3. Run Scenes/TestRunner.tscn"
    echo "   OR"
    echo "   Use: godot --headless --path . Scenes/TestRunner.tscn"
else
    echo -e "${RED}✗ Some tests failed${NC}"
    exit 1
fi
