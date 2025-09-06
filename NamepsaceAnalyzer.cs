using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NamespaceRestrictionAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ComprehensiveTypeUsageAnalyzer : DiagnosticAnalyzer
    {
        // Diagnostic IDs
        public const string NamespaceRestrictionDiagnosticId = "NS001";
        public const string TypeUsageRestrictionDiagnosticId = "TU001";
        public const string GenericTypeRestrictionDiagnosticId = "GT001";
        public const string DelegateUsageDiagnosticId = "DL001";
        public const string AttributeUsageDiagnosticId = "AT001";
        
        // Diagnostic descriptors
        private static readonly DiagnosticDescriptor NamespaceRestrictionRule = 
            new DiagnosticDescriptor(
                NamespaceRestrictionDiagnosticId,
                "Namespace usage violation",
                "Namespace '{0}' is not allowed in namespace '{1}'",
                "Design",
                DiagnosticSeverity.Error,
                true,
                "Restrict specific namespaces from being used in certain contexts.");

        private static readonly DiagnosticDescriptor TypeUsageRestrictionRule = 
            new DiagnosticDescriptor(
                TypeUsageRestrictionDiagnosticId,
                "Type usage violation",
                "Usage of type '{0}' is not allowed in '{1}'",
                "Design",
                DiagnosticSeverity.Error,
                true,
                "Restrict specific types from being used in certain contexts.");

        private static readonly DiagnosticDescriptor GenericTypeRestrictionRule = 
            new DiagnosticDescriptor(
                GenericTypeRestrictionDiagnosticId,
                "Generic type violation",
                "Usage of generic type '{0}' is not allowed in '{1}'",
                "Design",
                DiagnosticSeverity.Error,
                true,
                "Restrict specific generic types from being used in certain contexts.");

        private static readonly DiagnosticDescriptor DelegateUsageRule = 
            new DiagnosticDescriptor(
                DelegateUsageDiagnosticId,
                "Delegate usage violation",
                "Usage of delegate '{0}' is not allowed in '{1}'",
                "Design",
                DiagnosticSeverity.Error,
                true,
                "Restrict specific delegates from being used in certain contexts.");

        private static readonly DiagnosticDescriptor AttributeUsageRule = 
            new DiagnosticDescriptor(
                AttributeUsageDiagnosticId,
                "Attribute usage violation",
                "Usage of attribute '{0}' is not allowed in '{1}'",
                "Design",
                DiagnosticSeverity.Error,
                true,
                "Restrict specific attributes from being used in certain contexts.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
            ImmutableArray.Create(
                NamespaceRestrictionRule, 
                TypeUsageRestrictionRule,
                GenericTypeRestrictionRule,
                DelegateUsageRule,
                AttributeUsageRule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            
            // Register for namespace declaration analysis
            context.RegisterSyntaxNodeAction(AnalyzeNamespaceDeclaration, SyntaxKind.NamespaceDeclaration);
            
            // Register for various type usage patterns
            context.RegisterSyntaxNodeAction(AnalyzeObjectCreation, SyntaxKind.ObjectCreationExpression);
            context.RegisterSyntaxNodeAction(AnalyzeMethodCall, SyntaxKind.InvocationExpression);
            context.RegisterSyntaxNodeAction(AnalyzePropertyAccess, SyntaxKind.SimpleMemberAccessExpression);
            context.RegisterSyntaxNodeAction(AnalyzeFieldDeclaration, SyntaxKind.FieldDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeVariableDeclaration, SyntaxKind.VariableDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeParameterDeclaration, SyntaxKind.Parameter);
            context.RegisterSyntaxNodeAction(AnalyzeReturnType, SyntaxKind.MethodDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeReturnType, SyntaxKind.PropertyDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeReturnType, SyntaxKind.IndexerDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeBaseType, SyntaxKind.BaseList);
            context.RegisterSyntaxNodeAction(AnalyzeTypeOf, SyntaxKind.TypeOfExpression);
            context.RegisterSyntaxNodeAction(AnalyzeCastExpression, SyntaxKind.CastExpression);
            context.RegisterSyntaxNodeAction(AnalyzeAsExpression, SyntaxKind.AsExpression);
            context.RegisterSyntaxNodeAction(AnalyzeGenericName, SyntaxKind.GenericName);
            context.RegisterSyntaxNodeAction(AnalyzeDelegateDeclaration, SyntaxKind.DelegateDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeAttribute, SyntaxKind.Attribute);
            context.RegisterSyntaxNodeAction(AnalyzeLambdaExpression, SyntaxKind.ParenthesizedLambdaExpression);
            context.RegisterSyntaxNodeAction(AnalyzeLambdaExpression, SyntaxKind.SimpleLambdaExpression);
            context.RegisterSyntaxNodeAction(AnalyzeUsingDirective, SyntaxKind.UsingDirective);
            context.RegisterSyntaxNodeAction(AnalyzeArrayCreation, SyntaxKind.ArrayCreationExpression);
            context.RegisterSyntaxNodeAction(AnalyzeImplicitArrayCreation, SyntaxKind.ImplicitArrayCreationExpression);
            context.RegisterSyntaxNodeAction(AnalyzeDefaultExpression, SyntaxKind.DefaultExpression);
        }

        private void AnalyzeNamespaceDeclaration(SyntaxNodeAnalysisContext context)
        {
            var namespaceDecl = (NamespaceDeclarationSyntax)context.Node;
            var containingNamespace = namespaceDecl.Parent as NamespaceDeclarationSyntax;
            
            if (containingNamespace != null)
            {
                var containingNamespaceName = containingNamespace.Name.ToString();
                var currentNamespaceName = namespaceDecl.Name.ToString();
                
                if (!IsNamespaceAllowedInNamespace(currentNamespaceName, containingNamespaceName))
                {
                    var diagnostic = Diagnostic.Create(
                        NamespaceRestrictionRule,
                        namespaceDecl.Name.GetLocation(),
                        currentNamespaceName,
                        containingNamespaceName);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private void AnalyzeObjectCreation(SyntaxNodeAnalysisContext context)
        {
            var objectCreation = (ObjectCreationExpressionSyntax)context.Node;
            var typeSymbol = context.SemanticModel.GetTypeInfo(objectCreation).Type;
            
            if (typeSymbol != null)
            {
                CheckTypeUsage(context, typeSymbol, objectCreation.GetLocation());
            }
        }

        private void AnalyzeMethodCall(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            var methodSymbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
            
            if (methodSymbol != null)
            {
                CheckTypeUsage(context, methodSymbol.ContainingType, invocation.GetLocation());
                
                // Also check return type and parameter types
                CheckTypeUsage(context, methodSymbol.ReturnType, invocation.GetLocation());
                foreach (var parameter in methodSymbol.Parameters)
                {
                    CheckTypeUsage(context, parameter.Type, invocation.GetLocation());
                }
            }
        }

        private void AnalyzePropertyAccess(SyntaxNodeAnalysisContext context)
        {
            var memberAccess = (MemberAccessExpressionSyntax)context.Node;
            var symbol = context.SemanticModel.GetSymbolInfo(memberAccess).Symbol;
            
            if (symbol != null)
            {
                var containingType = symbol.ContainingType;
                if (containingType != null)
                {
                    CheckTypeUsage(context, containingType, memberAccess.GetLocation());
                }
                
                // If it's a property, check its type
                if (symbol is IPropertySymbol propertySymbol)
                {
                    CheckTypeUsage(context, propertySymbol.Type, memberAccess.GetLocation());
                }
            }
        }

        private void AnalyzeFieldDeclaration(SyntaxNodeAnalysisContext context)
        {
            var fieldDecl = (FieldDeclarationSyntax)context.Node;
            foreach (var variable in fieldDecl.Declaration.Variables)
            {
                var typeSymbol = context.SemanticModel.GetTypeInfo(fieldDecl.Declaration.Type).Type;
                if (typeSymbol != null)
                {
                    CheckTypeUsage(context, typeSymbol, fieldDecl.Declaration.Type.GetLocation());
                }
            }
        }

        private void AnalyzeVariableDeclaration(SyntaxNodeAnalysisContext context)
        {
            var variableDecl = (VariableDeclarationSyntax)context.Node;
            var typeSymbol = context.SemanticModel.GetTypeInfo(variableDecl.Type).Type;
            
            if (typeSymbol != null)
            {
                CheckTypeUsage(context, typeSymbol, variableDecl.Type.GetLocation());
            }
        }

        private void AnalyzeParameterDeclaration(SyntaxNodeAnalysisContext context)
        {
            var parameter = (ParameterSyntax)context.Node;
            var typeSymbol = context.SemanticModel.GetTypeInfo(parameter.Type).Type;
            
            if (typeSymbol != null)
            {
                CheckTypeUsage(context, typeSymbol, parameter.Type.GetLocation());
            }
        }

        private void AnalyzeReturnType(SyntaxNodeAnalysisContext context)
        {
            ITypeSymbol typeSymbol = null;
            Location location = null;
            
            if (context.Node is MethodDeclarationSyntax methodDecl)
            {
                typeSymbol = context.SemanticModel.GetTypeInfo(methodDecl.ReturnType).Type;
                location = methodDecl.ReturnType.GetLocation();
            }
            else if (context.Node is PropertyDeclarationSyntax propertyDecl)
            {
                typeSymbol = context.SemanticModel.GetTypeInfo(propertyDecl.Type).Type;
                location = propertyDecl.Type.GetLocation();
            }
            else if (context.Node is IndexerDeclarationSyntax indexerDecl)
            {
                typeSymbol = context.SemanticModel.GetTypeInfo(indexerDecl.Type).Type;
                location = indexerDecl.Type.GetLocation();
            }
            
            if (typeSymbol != null && location != null)
            {
                CheckTypeUsage(context, typeSymbol, location);
            }
        }

        private void AnalyzeBaseType(SyntaxNodeAnalysisContext context)
        {
            var baseList = (BaseListSyntax)context.Node;
            foreach (var baseType in baseList.Types)
            {
                var typeSymbol = context.SemanticModel.GetTypeInfo(baseType.Type).Type;
                if (typeSymbol != null)
                {
                    CheckTypeUsage(context, typeSymbol, baseType.Type.GetLocation());
                }
            }
        }

        private void AnalyzeTypeOf(SyntaxNodeAnalysisContext context)
        {
            var typeOfExpression = (TypeOfExpressionSyntax)context.Node;
            var typeSymbol = context.SemanticModel.GetTypeInfo(typeOfExpression.Type).Type;
            
            if (typeSymbol != null)
            {
                CheckTypeUsage(context, typeSymbol, typeOfExpression.Type.GetLocation());
            }
        }

        private void AnalyzeCastExpression(SyntaxNodeAnalysisContext context)
        {
            var castExpression = (CastExpressionSyntax)context.Node;
            var typeSymbol = context.SemanticModel.GetTypeInfo(castExpression.Type).Type;
            
            if (typeSymbol != null)
            {
                CheckTypeUsage(context, typeSymbol, castExpression.Type.GetLocation());
            }
        }

        private void AnalyzeAsExpression(SyntaxNodeAnalysisContext context)
        {
            var asExpression = (BinaryExpressionSyntax)context.Node;
            var typeSymbol = context.SemanticModel.GetTypeInfo(asExpression.Right).Type;
            
            if (typeSymbol != null)
            {
                CheckTypeUsage(context, typeSymbol, asExpression.Right.GetLocation());
            }
        }

        private void AnalyzeGenericName(SyntaxNodeAnalysisContext context)
        {
            var genericName = (GenericNameSyntax)context.Node;
            var symbolInfo = context.SemanticModel.GetSymbolInfo(genericName);
            
            if (symbolInfo.Symbol is INamedTypeSymbol namedTypeSymbol)
            {
                CheckTypeUsage(context, namedTypeSymbol, genericName.GetLocation());
                
                // Check type arguments
                foreach (var typeArgument in namedTypeSymbol.TypeArguments)
                {
                    CheckTypeUsage(context, typeArgument, genericName.GetLocation());
                }
            }
        }

        private void AnalyzeDelegateDeclaration(SyntaxNodeAnalysisContext context)
        {
            var delegateDecl = (DelegateDeclarationSyntax)context.Node;
            var delegateSymbol = context.SemanticModel.GetDeclaredSymbol(delegateDecl);
            
            if (delegateSymbol != null)
            {
                CheckTypeUsage(context, delegateSymbol, delegateDecl.Identifier.GetLocation());
                
                // Check return type
                CheckTypeUsage(context, delegateSymbol.ReturnType, delegateDecl.ReturnType.GetLocation());
                
                // Check parameter types
                foreach (var parameter in delegateSymbol.Parameters)
                {
                    CheckTypeUsage(context, parameter.Type, delegateDecl.ParameterList.GetLocation());
                }
            }
        }

        private void AnalyzeAttribute(SyntaxNodeAnalysisContext context)
        {
            var attribute = (AttributeSyntax)context.Node;
            var attributeSymbol = context.SemanticModel.GetSymbolInfo(attribute).Symbol;
            
            if (attributeSymbol != null && attributeSymbol.ContainingType != null)
            {
                CheckTypeUsage(context, attributeSymbol.ContainingType, attribute.Name.GetLocation());
            }
        }

        private void AnalyzeLambdaExpression(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is LambdaExpressionSyntax lambda)
            {
                var lambdaSymbol = context.SemanticModel.GetSymbolInfo(lambda).Symbol;
                
                if (lambdaSymbol is IMethodSymbol methodSymbol)
                {
                    CheckTypeUsage(context, methodSymbol.ReturnType, lambda.GetLocation());
                    
                    foreach (var parameter in methodSymbol.Parameters)
                    {
                        CheckTypeUsage(context, parameter.Type, lambda.GetLocation());
                    }
                }
            }
        }

        private void AnalyzeUsingDirective(SyntaxNodeAnalysisContext context)
        {
            var usingDirective = (UsingDirectiveSyntax)context.Node;
            if (usingDirective.Name != null)
            {
                var namespaceSymbol = context.SemanticModel.GetSymbolInfo(usingDirective.Name).Symbol as INamespaceSymbol;
                
                if (namespaceSymbol != null)
                {
                    // Check if this namespace is allowed in the current context
                    var containingNamespace = context.ContainingSymbol?.ContainingNamespace?.ToString();
                    if (containingNamespace != null && !IsNamespaceAllowedInNamespace(namespaceSymbol.ToString(), containingNamespace))
                    {
                        var diagnostic = Diagnostic.Create(
                            NamespaceRestrictionRule,
                            usingDirective.Name.GetLocation(),
                            namespaceSymbol.ToString(),
                            containingNamespace);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }

        private void AnalyzeArrayCreation(SyntaxNodeAnalysisContext context)
        {
            var arrayCreation = (ArrayCreationExpressionSyntax)context.Node;
            var typeSymbol = context.SemanticModel.GetTypeInfo(arrayCreation.Type).Type;
            
            if (typeSymbol != null)
            {
                CheckTypeUsage(context, typeSymbol, arrayCreation.Type.GetLocation());
            }
        }

        private void AnalyzeImplicitArrayCreation(SyntaxNodeAnalysisContext context)
        {
            var implicitArrayCreation = (ImplicitArrayCreationExpressionSyntax)context.Node;
            var typeInfo = context.SemanticModel.GetTypeInfo(implicitArrayCreation);
            
            if (typeInfo.Type is IArrayTypeSymbol arrayTypeSymbol)
            {
                CheckTypeUsage(context, arrayTypeSymbol.ElementType, implicitArrayCreation.GetLocation());
            }
        }

        private void AnalyzeDefaultExpression(SyntaxNodeAnalysisContext context)
        {
            var defaultExpression = (DefaultExpressionSyntax)context.Node;
            var typeSymbol = context.SemanticModel.GetTypeInfo(defaultExpression.Type).Type;
            
            if (typeSymbol != null)
            {
                CheckTypeUsage(context, typeSymbol, defaultExpression.Type.GetLocation());
            }
        }

        // Main function to check type usage permissions
        private void CheckTypeUsage(SyntaxNodeAnalysisContext context, ISymbol usedSymbol, Location location)
        {
            // Get the containing symbol (method, class, etc.) where the type is being used
            var containingSymbol = context.ContainingSymbol;
            
            if (containingSymbol != null && usedSymbol != null)
            {
                // Check if this symbol usage is allowed in the current context
                if (!IsSymbolUsageAllowed(containingSymbol, usedSymbol))
                {
                    var diagnostic = Diagnostic.Create(
                        TypeUsageRestrictionRule,
                        location,
                        usedSymbol.Name,
                        containingSymbol.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private void CheckTypeUsage(SyntaxNodeAnalysisContext context, ITypeSymbol usedType, Location location)
        {
            // Get the containing symbol (method, class, etc.) where the type is being used
            var containingSymbol = context.ContainingSymbol;
            
            if (containingSymbol != null && usedType != null)
            {
                // Check if this type usage is allowed in the current context
                if (!IsTypeUsageAllowed(containingSymbol, usedType))
                {
                    var diagnostic = Diagnostic.Create(
                        TypeUsageRestrictionRule,
                        location,
                        usedType.Name,
                        containingSymbol.Name);
                    context.ReportDiagnostic(diagnostic);
                }
                
                // Check for generic types
                if (usedType is INamedTypeSymbol namedType && namedType.IsGenericType)
                {
                    foreach (var typeArgument in namedType.TypeArguments)
                    {
                        CheckTypeUsage(context, typeArgument, location);
                    }
                }
            }
        }

        // Define your namespace restriction rules here
        private bool IsNamespaceAllowedInNamespace(string innerNamespace, string outerNamespace)
        {
            // Example restriction: System.* namespaces cannot be used within Company.Product.*
            if (outerNamespace.StartsWith("Company.Product") && innerNamespace.StartsWith("System"))
            {
                return false;
            }
            
            // Add other namespace restriction rules as needed
            return true;
        }

        // Define your type usage restriction rules here
        private bool IsTypeUsageAllowed(ISymbol callingSymbol, ITypeSymbol usedType)
        {
            // Example restriction: Check if specific types are used in specific contexts
            if (callingSymbol.ContainingType?.Name == "RestrictedClass" && 
                usedType.Name == "ForbiddenType")
            {
                return false;
            }
            
            // Example: Prevent using System.IO in certain methods
            if (usedType.ContainingNamespace?.ToString().StartsWith("System.IO") == true &&
                callingSymbol.Name == "SensitiveMethod")
            {
                return false;
            }
            
            // Add other type usage restriction rules as needed
            return true;
        }

        private bool IsSymbolUsageAllowed(ISymbol callingSymbol, ISymbol usedSymbol)
        {
            // Example restriction: Check if specific symbols are used in specific contexts
            if (callingSymbol.ContainingType?.Name == "RestrictedClass" && 
                usedSymbol.Name == "ForbiddenMethod")
            {
                return false;
            }
            
            // Add other symbol usage restriction rules as needed
            return true;
        }
    }
}

