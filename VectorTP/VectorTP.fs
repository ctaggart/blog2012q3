namespace VectorTP

open System
open System.Reflection
open Microsoft.FSharp.Core.CompilerServices

module Helpers =
    let stringParameter index defaultVal =
        { new ParameterInfo() with
            override this.Name with get() = sprintf "axis%d" index
            override this.ParameterType with get() = typeof<string>
            override this.Position with get() = 0
            override this.RawDefaultValue with get() = defaultVal
            override this.DefaultValue with get() = defaultVal
            override this.Attributes with get() = ParameterAttributes.Optional
        }

    let makeClass body name =
        let code = "namespace Mindscape.Vectorama { public class " + name + " {" + Environment.NewLine + body + Environment.NewLine + "} }"
        let dllFile = System.IO.Path.GetTempFileName()
        let csc = new Microsoft.CSharp.CSharpCodeProvider()
        let parameters = new System.CodeDom.Compiler.CompilerParameters()
        parameters.OutputAssembly <- dllFile
        parameters.CompilerOptions <- "/t:library"
        // Ignoring error checking
        let compilerResults = csc.CompileAssemblyFromSource(parameters, [| code |])
        let asm = compilerResults.CompiledAssembly
        asm.GetType("Mindscape.Vectorama." + name)

    let makeVector name argnames =
        let propNames =
            argnames
            |> Seq.filter (fun arg -> arg <> null && not (String.IsNullOrWhiteSpace(arg.ToString())))
            |> Seq.map (fun arg -> arg.ToString())
            |> Seq.toList
        let props =
            propNames
            |> List.map (fun arg -> "public double " + arg + " { get; set; }")
            |> String.concat Environment.NewLine
        let dotProductBody =
            propNames
            |> List.map (fun arg -> sprintf "this.%s * other.%s" arg arg)
            |> String.concat " + "
        let dotProduct = sprintf "public double DotProduct(%s other) { return %s; }" name dotProductBody
        let body = props + Environment.NewLine + dotProduct
        makeClass body name
 
open Helpers

// Cheating for simplicity
type Vector() = inherit obj()
 
[<TypeProvider>]
type VectorProvider() =

    let invalidation = new Event<_,_>()
    
    interface ITypeProvider with
        
        [<CLIEvent>]
        member this.Invalidate =
            //Diagnostics.Debugger.Break()
            printfn "ITypeProvider.Invalidate"
            invalidation.Publish

        member this.GetNamespaces() =
            printfn "ITypeProvider.GetNamespaces()"
            [| this |]
        
        member this.GetStaticParameters(typeWithoutArguments) =
            printfn "ITypeProvider.GetStaticParameters(%A)" typeWithoutArguments
            [1..7] |> List.map (fun i -> stringParameter i "") |> List.toArray

        member this.ApplyStaticArguments(typeWithoutArguments, typeNameWithArguments, staticArguments) =
            printfn "ITypeProvider.ApplyStaticArguments(%A, %A, %A)" typeWithoutArguments typeNameWithArguments staticArguments
            makeVector typeNameWithArguments.[typeNameWithArguments.Length-1] staticArguments

        member this.GetInvokerExpression(syntheticMethodBase, parameters) =
            printfn "ITypeProvider.GetInvokerExpression(%A, %A)" syntheticMethodBase parameters
            // this method only needs to be implemented for a generated type provider if
            // you are using the declared types from in the same project or in a script
            //NotImplementedException(sprintf "Not Implemented: ITypeProvider.GetInvokerExpression(%A, %A)" syntheticMethodBase parameters) |> raise
            match syntheticMethodBase with
            | :? ConstructorInfo as ctor ->
                Quotations.Expr.NewObject(ctor, Array.toList parameters) 
            | :? MethodInfo as mi ->
                Quotations.Expr.Call(parameters.[0], mi, Array.toList parameters.[1..])
            | _ ->
                NotImplementedException(sprintf "Not Implemented: ITypeProvider.GetInvokerExpression(%A, %A)" syntheticMethodBase parameters) |> raise

        member this.GetGeneratedAssemblyContents(assembly) =
            printfn "ITypeProvider.GetGeneratedAssemblyContents(%A)" assembly
            printfn "  ReadAllBytes %s" assembly.ManifestModule.FullyQualifiedName
            IO.File.ReadAllBytes assembly.ManifestModule.FullyQualifiedName

        member this.Dispose() =
            printfn "ITypeProvider.Dispose()"
            ()

    interface IProvidedNamespace with

        member this.ResolveTypeName(typeName) =
            printfn "IProvidedNamespace.ResolveTypeName(%A)" typeName
            typeof<Vector>

        member this.NamespaceName
            with get() =
                printfn "IProvidedNamespace.NamespaceName.get()";
                "Mindscape.Vectorama"

        member this.GetNestedNamespaces() =
            printfn "IProvidedNamespace.GetNestedNamespaces()"
            [| |]

        member this.GetTypes() =
            printfn "IProvidedNamespace.GetTypes()"
            [| typeof<Vector> |]

[<assembly: TypeProviderAssembly>]
do ()