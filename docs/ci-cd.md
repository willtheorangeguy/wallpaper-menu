# CI/CD

GitHub Actions handles continuous integration (CI), release builds, and site deployment.

| Workflow | Trigger | Result |
| --- | --- | --- |
| Windows tests | Push, pull request or manual run | Builds the application and runs the test suite on Windows. |
| Windows installer | Release publication or manual run | Tests the build, then attaches the executable to the release or saves it as a run artifact. |
| Docs | Documentation changes on the default branch or manual run | Runs the shared strict MkDocs build and deploys to GitHub Pages. |
| Docs Lint | Documentation pull request or manual run | Checks Markdown style, builds the site in strict mode and checks external links. |

## Release builds

Publish a GitHub release pointing at the commit you intend to ship. The installer workflow checks out the release tag, runs `tests/Run-Tests.ps1`, and attaches `WallpaperMenu-Setup.exe` to the release.

For a build to inspect before publishing, open **Actions** → **Windows installer** → **Run workflow**. Download the `WallpaperMenu-Setup` artifact from the successful run.

## Documentation deployment

`mkdocs.yml` contains the project navigation, canonical URL and repository details. The reusable [MkDocs workflows](https://github.com/willtheorangeguy/mkdocs) supply the build, style checks, shared theme and deployment. See [Developer guide](./development.md#preview-the-docs) for a local build.
