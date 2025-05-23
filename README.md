# SharedBlazorCompoments
A model first approach to an mvvm  implementation for shared Blazor compoments

## GitHub Actions Workflows

This repository contains two GitHub Actions workflows:

1. **Deploy Workflow** (`.github/workflows/deploy.yml`): This workflow builds and pushes the `Innovative.Blazor.Components` package. It is triggered on pushes to the `main`, `develop`, and `release*` branches, as well as manual triggers. The workflow includes steps for extracting version information, building the solution, running tests, and publishing the package to NuGet.org.

2. **Test Workflow** (`.github/workflows/test.yml`): This workflow runs tests and checks code coverage. It is triggered on pull requests to the `main` and `develop` branches. The workflow includes steps for restoring dependencies, building the solution, running tests, reporting code coverage, and checking if the total code coverage is below 75%. If the code coverage is below 75%, the workflow will fail.
