using System;
using System.Linq;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis;

public static class Program
{
    static async Task Main(string[] args)
    {
        // Register MSBuild
        MSBuildLocator.RegisterDefaults();

        using var workspace = MSBuildWorkspace.Create();
        var solutionPath = @"C:\\Users\\av\\Code\\MyRepositories\\SourceCodeGenerator\\ConsoleApp1\\ConsoleApp1.sln";

        // Open the solution
        var solution = await workspace.OpenSolutionAsync(solutionPath);

        // Process each project in the solution
        foreach (var project in solution.Projects)
        {
            Console.WriteLine($"Project: {project.Name}");
            await ProcessProjectAsync(project);
        }
    }

    static async Task ProcessProjectAsync(Project project)
    {
        var compilation = await project.GetCompilationAsync();
        if (compilation == null) return;

        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var root = await syntaxTree.GetRootAsync();

            // Extract all type declarations (classes, structs, enums)
            var typeDeclarations = root.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax>();

            foreach (var typeDeclaration in typeDeclarations)
            {
                var symbol = semanticModel.GetDeclaredSymbol(typeDeclaration);
                if (symbol != null)
                {
                    // Check if the type is decorated with ReflectAttribute
                    var hasReflectAttribute = symbol.GetAttributes().Any(attr => attr.AttributeClass.Name == "ReflectAttribute");
                    if (hasReflectAttribute)
                    {
                        Console.WriteLine($"Type: {symbol.Name} is decorated with ReflectAttribute");
                    }
                }
            }
        }
    }
}
