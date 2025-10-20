**RaviinLib.CAS** is a fully custom built ***[Computer Algebra System](https://en.wikipedia.org/wiki/Computer_algebra_system)***.

**This library supports:**  
String Parsing  
Triginometric Functions (and others)  
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
  
  
*This README is ongoing and is **NOT** Complete*

# Getting Started

### Functions
The Function class is the main class of the library. Everything revolves around it.  
#### Constructing Functions
```cs
Function Ex1 = new Function("x^2", new List<string>() { "x" }); // Manually set variables
Function Ex2 = new Function("x^2"); // Auto detect variables
Function Ex3 = "x^2"; // Auto detect variables
```

##### Mixing Functions
```cs
Function Ex4 = ( Ex1 + "2" ) / ( Ex2 + 1 ); // ( x^2 + 2 ) / ( x^2 + 1 )
```

#### Common Function Methods
```cs
Function Ex = "x^2 * x";

Function Simplified = Ex.Simplified;
Function Expanded = Ex.Expanded;
Function[] Gradiant = Ex.Gradiant;
Function Derivative = Ex.Derivative("x");
Function SimplifiedDerivative = Derivative.Simplified;
double Subs = Ex.Subs(2);
Function Replace = Ex.Replace("x", "y");
Function ReplaceAlternative = Ex["x","y"];
```

#### Viewing Common Function Methods
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

# Functions In Depth

#### Properties vs Methods
Most of the available processes for functions come in both Method and Property form.
For example:
```cs
Ex.GetSimplified();
Ex.Simplified;
```
These both call the same underlying functions. However, if we look one level under the hood, "Ex.Simplified" calls "GetSimplified()" and caches this value for later access. That way the next time "Ex.Simplified" is called there is no calculation overhead.
```cs
private Function _Simplified = null;
public Function Simplified
{
    get
    {
        if (_Simplified == null) _Simplified = GetSimplified();
        return _Simplified;
    }
}
```

Another important note is all calculations are returned as a new object.  
The original Function is never modified. This means you can chain calculations without worrying about modifying the original function.
The only exception to this is the "Function.Simplify()" method, which modifies the original function in place.
```cs
Function Ex = "x^2 * x";
Function Ex2 = Ex.Simplified.Expanded.Derivative("x").Simplified; // Chains calculations without modifying Ex
Console.WriteLine($"Ex: {Ex}");
Console.WriteLine($"Ex2: {Ex2}");

Console.WriteLine($"Before Simplify(): {Ex}");
Ex.Simplify(); // Modifies Ex in place
Console.WriteLine($"After Simplify(): {Ex}");
```
```
Ex: x^2 * x
Ex2: 3x^2
Before Simplify(): x^2 * x
After Simplify(): x^3
```


#### Subs()
"Subs()" is used when you need to substitute values into a funcation and you need a number as an output. This function requires **ALL** variables to have a valid substition, as otherwise this calculation would be impossible.  
For example:
```cs
Function ExSubs = "x + y";

Dictionary<string,double> Dict = new Dictionary<string,double>() { {"x",2} , {"y",1} };
ExSubs.Subs(Dict); // Correct

Dictionary<string,double> IncorrectDict = new Dictionary<string,double>() { {"x",2} };
ExSubs.Subs(IncorrectDict); // Incorrect
```
If creating a dictionary every time is annoying don't worry! If you know the order of the variables list (Function.Variables) then you can just pass them in as a list!  
```cs
List<double> Vals = new List<double>(){2,1};
ExSubs.Subs(Vals); // "x" = 2 | "y" = 1
```
If your function only contains one variable you can also just pass in a double without creating a List or Dictionary.
```cs
Function ExSubs2 = "x";
ExSubs2.Subs(2);
```
If your Function has several variables, but you want to subsitute all of them to the same value you can use "Function.SubsAll()". This will substitute the value into all variables.
```cs
ExSubs.SubsAll(2); // "x" = 2 | "y" = 2
```

#### Replace()
Replace works very simmilar to Subs(). Except instead of returning a double, it returns another Function. This has the advantage of not requiring all variables to be replaced, and whole Functions can be replaced into other functions!
```cs
Function ExReplace = "x^2 + y";
Function ExReplace2 = "0.25z";

Dictionary<string,Function> Dict = new Dictionary<string,Function>() { {"x", ExReplace2} };
ExReplace.Replace(Dict); // (0.25z)^2 + y
```
Alternatives for parameters of the Replace() function follow the same scheme as Subs().  
In addition to the others Subs() has, Replace() has a shortcut syntax for cleaner visual chains of replacement.
```cs
Function ExReplace3 = "x + y + z";
_ = ExReplace3["x","g^2"]["y","g"]["z","1"]; // (g^2)^1 + (g)^1 + (1)^1
```

#### GetTaylorAproximation()
The Taylor Series Aproximations are used to aproximate functions as polynomials around a center point.

This is a classic example aproximating Sin(x).
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

# Graphing  
If you are needing to graph a function, I highly recomend the ScottPlot library. ScottPlot has built in "Function" ploting which makes graphing extremely easy!  
ScottPlot also has prebuilt UserControls for most of the common application developement platforms such as WPF, Maui, and Blazor for displaying the graphs natively.
  
The method for adding functions to a plot is "Plot.Add.Function()", where the method takes in a Func<double,double> which can easly be made by creating a lambda expression that calls the Function.Subs() method.  
```cs
ScottPlot.Plot Plot = new ScottPlot.Plot();
Function Fx = "Sin(x)";
Plot.Add.Function((x) => Fx.Subs(x));
```
  
This is an example graphing multiple Taylor Series Aproximations of Sin(x) on a single graph.  
```cs
void Plot(List<Function> Functions, ScottPlot.Range xRange, ScottPlot.Range yRange, string Path)
{
    ScottPlot.Plot Plot = new ScottPlot.Plot();

    foreach (var Fx in Functions)
    {
        Plot.Add.Function((x) => Fx.Subs(x)); // <-- Adding Functions to the plot
    }

    Plot.Axes.SetLimits(xRange.Min, xRange.Max, yRange.Min, yRange.Max);

    Plot.SavePng(Path, 400, 300);
}

Function f = "Sin(x)";
Function f1 = f.GetTaylorAproximation(0,1);
Function f2 = f.GetTaylorAproximation(0,2);
Function f3 = f.GetTaylorAproximation(0,3);
Function f15 = f.GetTaylorAproximation(0,15);

List<Function> Funcs = new() {f,f1,f3,f15};

Plot(Funcs, new ScottPlot.Range(-10, 10), new ScottPlot.Range(10,10), @"C:\Users\Raviin\Downloads\Plot.png");
```
![ Plot example image failed to load... :'( ](https://raw.githubusercontent.com/Raviin15/RaviinLib.CAS/refs/heads/master/RaviinLib.CAS/ReadMeReferences/PlotExample.png)
