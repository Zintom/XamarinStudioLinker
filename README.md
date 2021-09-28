# Xamarin Studio Linker
![Application Logo](https://zintom.gallerycdn.vsassets.io/extensions/zintom/xamarinstudiolinker/1.1/1632793280896/Microsoft.VisualStudio.Services.Icons.Default)

Opens .xml files in Android Studio or Intellij IDEA.

Creates a temporary **Android Studio/IDEA** project and uses a *symbolic link* to link your **Xamarin.Android** resources folder with that temporary project, conveniently launches Android Studio into the project, any changes there will instantly reflect in your Xamarin.Android project.

**Requires Visual Studio to be running as Administrator** ([CreateSymbolicLinkW](https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-createsymboliclinkw) prerequisite)

Download from the [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=Zintom.XamarinStudioLinker).

# Impetus
Xamarin.Android has come a *long* way, however the xml "designer" is pretty lackluster, by default it doesn't even support **ConstraintLayout**, yes there is an extension for it however it just provides basic support, no where near matching the Android Studio designer. The main points against it are the fact that you can't specify a 0dp width for views which is *required* in many cases for ConstraintLayout, also there is no code formatting and the auto-complete is, well, not complete 😀

Rather than implement my own layout editor, the simple and most effective solution was to use the Android Studio implementation.
