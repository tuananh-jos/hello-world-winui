using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Controls;

namespace App7.Presentation.Controls;

/// <summary>
/// A Button that shows a Hand cursor on hover.
/// Use this instead of &lt;Button&gt; anywhere a hand cursor is desired.
/// </summary>
public class HandButton : Button
{
    public HandButton()
    {
        DefaultStyleKey = typeof(Button);
        ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Hand);
    }
}
