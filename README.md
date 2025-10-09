**RaviinLib.CAS** is a fully custom built ***[Computer Algebra System](https://en.wikipedia.org/wiki/Computer_algebra_system)***.

**This library supports:**  
String Parsing  
Trigiometric Functions (and others)  
Derivatives (Including Partial)  
Antiderivatives (Basic)  
Function Expansion  
Function Simplification  
LaTeX Conversion  
Substitution and Replacement  
Unit Handling  
Piecewise Linear Aproximations  
Taylor Series Aproximations  
UFT-16 Character Set  
and More!  

# Basics

### Functions
The Function class is the main class of the library. Everything revolves around it.  
##### Constructing Functions
```cs
Function Ex1 = new Function("x^2", new List<string>() { "x" }); // Manually set variables
Function Ex2 = new Function("x^2"); // Auto detect variables
Function Ex3 = "x^2"; // Auto detect variables
```

##### Mixing Functions
```cs
Function Ex4 = ( Ex1 + "2" ) / ( Ex2 + 1 ); // ( x^2 + 2 ) / ( x^2 + 1 )
```

##### Common Function Methods
```cs
Function Ex = "x^2 * x";

Function Simplified = Ex.Simplified; // Simplifies the function
Function Expanded = Ex.Expanded; // Expands the function
Function[] Gradiant = Ex.Gradiant; // Gets the gradiant of the function (array of partial derivatives)
Function Derivative = Ex.Derivative("x"); // Gets the derivative with respect to x
Function SimplifiedDerivative = Derivative.Simplified; // Simplifies the derivative
double Subs = Ex.Subs(2); // Substitutes x = 2 into the function
Function Replace = Ex.Replace("x", "y"); // Replaces variable x with y
Function ReplaceAlternative = Ex["x","y"]; // Replaces variable x with y (alternate syntax)
```

##### Viewing Common Function Methods
```cs
Console.WriteLine($"Original: {Ex}");
Console.WriteLine($"Simplified: {Simplified}");
Console.WriteLine($"Expanded: {Expanded}");
Console.WriteLine($"Gradiant: {Gradiant}");
Console.WriteLine($"Derivative: {Derivative}");
Console.WriteLine($"SimplifiedDerivative: {SimplifiedDerivative}");
Console.WriteLine($"Substitution: {Subs}");
Console.WriteLine($"Replace: {Replace}");
Console.WriteLine($"ReplaceAlternative: {ReplaceAlternative}");
```
```
Original: x^2 * x
Simplified: x^3
Expanded: x * x * x
Gradiant: RaviinLib.CAS.Function[]
Derivative: 2x * x + x^2 * 1
SimplifiedDerivative: 3x^2
Substitution: 8
Replace: (y)^2 * (y)^1
ReplaceAlternative: (y)^2 * (y)^1
```

##### Taylor Series Aproximation
```cs
Function Sin = "Sin(x)";
double Center = 0;
int Order = 3;
Function SinAprox = Sin.GetTaylorAproximation(Center,Order);
Console.WriteLine($"Aprox: {SinAprox.Simplified}");
```
```
Aprox: -0.16666666666666666x^3 + x
```
  
If speed is what you're after and not accuracy consider using aproximate factorial calculation.  
This is only recomended for large functions that grow after derivation.  
```cs
SinAprox = Sin.GetTaylorAproximation(Center,Order,true);
Console.WriteLine($"Aprox: {SinAprox.Simplified}");
```
```
Aprox: -0.19865947308564935x^3 + 0.6577446234794568x
```
