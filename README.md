# Xamarin Studio Linker
![Application Logo](https://zintom.gallerycdn.vsassets.io/extensions/zintom/xamarinstudiolinker/1.1/1632793280896/Microsoft.VisualStudio.Services.Icons.Default)

Edit your resource files in the **Android Studio/IDEA** IDE and watch the changes instantly reflect in your **Xamarin.Android** project.

Creates a temporary Android Studio/Intellij IDEA project and uses a *symbolic link* which links your Xamarin.Android `Resources` folder with the temporary project, it will then conveniently launch Android Studio into the project whereby you can edit your resources (layout, colours, vector assets, etc), any changes will instantly reflect in your Xamarin.Android project.

**Requires Visual Studio to be running as Administrator** ([CreateSymbolicLinkW](https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-createsymboliclinkw) prerequisite).

Download from the [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=Zintom.XamarinStudioLinker).

## Impetus
Xamarin.Android has come a *long* way, however the xml "designer" is pretty lackluster, by default it doesn't even support `ConstraintLayout`, yes there is an extension for it however it just provides basic support, no where near matching the Android Studio designer. The main points against it are the fact that you can't specify a 0dp width for views which is *required* in many cases for ConstraintLayout, also there is no code formatting and the auto-complete is, well, not complete 😀

Rather than implement my own layout editor, the simple and most effective solution was to use the Android Studio implementation.

## Common Headaches
* If you create new files in the Android Studio IDE then you will need to make them visible to the Visual Studio project by using `Add > Existing Item` in the solution explorer. I would recommend just creating the files in Visual Studio if you wish to avoid this issue.
* Some views need you to use AppCompat.Theme to show themselves correctly in the Android Studio layout editor. You can do this by setting the theme directly in the editor. *(at some point in the future I may include a theme file that defaults to AppCompat)*.
