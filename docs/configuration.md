# Configuration

Configure the slideshow in Windows Settings. Wallpaper Menu gets the active image from Windows and checks that the file is inside the `Wallpapers` folder in your account's Pictures folder.

## Windows background settings

| Option | Type | Default | Description |
| --- | --- | --- | --- |
| Personalize your background | Mode | Current Windows setting | Choose **Slideshow**. |
| Slideshow picture album | Directory | Current Windows setting | Select **Pictures** → **Wallpapers**. |
| Change picture every | Interval | Current Windows setting | Choose when Windows advances on its own, for example **10 minutes**. |
| Shuffle the picture order | Toggle | Current Windows setting | Turn on to let Windows shuffle the photos. |

## Eligible files

The current wallpaper must resolve to an existing file inside the account's `Pictures\Wallpapers` folder. The app accepts files in subfolders when Windows reports them as the active image. It rejects linked files and linked directories within the wallpaper folder.

The extension check accepts `.jpg`, `.jpeg`, `.jfif`, `.png`, `.bmp`, `.gif`, `.tif`, `.tiff`, `.webp`, `.heic` and `.avif`, regardless of case. Windows determines which images it can display in the slideshow.

## Example

Set **Personalize your background** to **Slideshow**, choose **Pictures** → **Wallpapers**, set the interval to **10 minutes** and turn on shuffle. A current wallpaper such as `Pictures\Wallpapers\Museum.JPG` passes the extension check. The rest of the validation runs when you use the menu.
