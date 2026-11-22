# Wallpaper Menu — Known issues

## Slideshow refresh can fail without a status message

| Field | Detail |
| --- | --- |
| Severity | Low |
| Where | `WallpaperMenu.cs:165–187`, `RefreshMonitor` |
| What | After recycling the photo, the refresh helper returns silently if Windows rejects a fallback advance or the attempt limit is reached with the original image still showing. |
| Why it matters | The desktop can keep displaying a cached copy, leaving the user unsure whether the file was recycled. |
| Suggested fix | Return the refresh outcome to the removal handler and show the existing **Next desktop background** instruction when the selected display still shows the previous image. |

See [Troubleshooting](../troubleshooting.md#the-photo-stays-on-screen-after-removal) for the workaround.
