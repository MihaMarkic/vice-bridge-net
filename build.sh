#!/bin/bash
SCRIPT_NAME="recipe.cake"


echo "Bootstrapping Cake"
dotnet cake $SCRIPT_NAME --bootstrap

echo "Running Build"
dotnet cake $SCRIPT_NAME "$@"