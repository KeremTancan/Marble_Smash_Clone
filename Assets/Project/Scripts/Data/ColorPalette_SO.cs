using System.Collections.Generic;
using UnityEngine;

/// Oyun içinde kullanılabilecek renklerin bir listesini tutan veri asset'i.

[CreateAssetMenu(fileName = "Palette_Default", menuName = "Marble Smash/Color Palette")]
public class ColorPalette_SO : ScriptableObject
{
    public List<Color> Colors;
}
