/// <summary>
/// Generic shape types that can represent different shapes in different levels.
/// For example:
/// Level 1: Shape1=Square, Shape2=Circle, etc.
/// Level 2: Shape1=Apple, Shape2=Banana, etc.
/// Level 3: Shape1=Lion, Shape2=Elephant, etc.
/// Just match the numbers between shapes and slots (Shape1 goes to Slot1).
/// </summary>
public enum ShapeType
{
    Shape1,     // First pair in each level
    Shape2,     // Second pair in each level
    Shape3,     // Third pair in each level
    Shape4,     // Fourth pair in each level
    Shape5,     // Fifth pair in each level
    Shape6,     // Sixth pair in each level
    Shape7,     // Seventh pair in each level
    Shape8,     // Eighth pair in each level
    Shape9,     // Ninth pair in each level
    Shape10,    // Tenth pair in each level
    Shape11,    // Additional shapes can be added as needed
    Shape12,    // Just add more shapes and Unity will auto-update
    None        // Used for initialization or invalid states
}
