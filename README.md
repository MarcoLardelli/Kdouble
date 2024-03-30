# A simple symbolic math plug-in replacement for 'double'

This is a prototype for a class ('Kdouble') which can be used instead of 'double' with a matching class 'KMath' which can be used instead of 'Math'.

The idea was to store the results of calculations (including input variables) as strings instead of computing them immediately numerically. The system also creates automatically variables for expressions which i tries to re-use to reduce the number of calculations to a minimum.

The indented use case was *procedural graphics*: You can use Kdouble's to calculate - e.g. - a 3D mesh which depends on some input variables. Then you can use the resulting expressions (which are again C# code) to re-calculate meshes with various input parameters very quickly.

Keep in mind that the **memory consumption** of a calculation based on Kdouble is typically **much larger** compared to normal double.

The code seems to work, but it is not finished. But in case you are planning to develop something similar, the code in this repo might be a good start or serve as inspiration.