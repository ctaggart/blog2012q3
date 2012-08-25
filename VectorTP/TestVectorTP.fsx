#r "bin\Debug\VectorTP.dll"
type Vector2D = Mindscape.Vectorama.Vector<"X", "Y">
let v = Vector2D()
v.X <- 100.0
v.Y <- 200.0
printfn "Vector X:%f , Y: %f" v.X v.Y

