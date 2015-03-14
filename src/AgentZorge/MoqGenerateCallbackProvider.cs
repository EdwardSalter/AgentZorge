using System.Linq;
using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.CodeCompletion;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure;
using JetBrains.ReSharper.Feature.Services.CSharp.CodeCompletion.Infrastructure;
using JetBrains.ReSharper.Feature.Services.Lookup;
using JetBrains.ReSharper.Feature.Services.VB.CodeCompletion.LookupItems;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Caches2;
using JetBrains.ReSharper.Psi.Tree;

namespace AgentZorge
{
    [Language(typeof (CSharpLanguage))]
    public class MoqGenerateCallbackProvider : ItemsProviderOfSpecificContext<CSharpCodeCompletionContext>
    {
        protected override bool IsAvailable(CSharpCodeCompletionContext context)
        {
            CodeCompletionType codeCompletionType = context.BasicContext.CodeCompletionType;
            return codeCompletionType == CodeCompletionType.SmartCompletion || codeCompletionType == CodeCompletionType.BasicCompletion;
        }

        protected override bool AddLookupItems(CSharpCodeCompletionContext context, GroupedItemsCollector collector)
        {
            AddLookupItemsNew(context, collector);
            return true;
        }

        private void AddLookupItemsNew([NotNull] CSharpCodeCompletionContext context, [NotNull] GroupedItemsCollector collector)
        {
            if (context.TerminatedContext == null)
                return;
            var identifier = context.TerminatedContext.TreeNode as IIdentifier;
            var mockedMethodArgument = identifier
                .GetParentSafe<IReferenceExpression>()
                .GetParentSafe<ICSharpArgument>();
            if (mockedMethodArgument == null)
                return;
            var callbackInvocationExpression = mockedMethodArgument
                .GetParentSafe<IArgumentList>()
                .GetParentSafe<IInvocationExpression>();
            if (callbackInvocationExpression == null || !callbackInvocationExpression.IsMoqCallbackMethod())
                return;
            var invokedExpression = callbackInvocationExpression.InvokedExpression as IReferenceExpression;
            if (invokedExpression == null)
                return;
            var setupOrReturnInvocationExpression = invokedExpression.QualifierExpression as IInvocationExpression;
            IInvocationExpression setupInvocationExpression;
            if (setupOrReturnInvocationExpression.IsMoqSetupMethod())
            {
                setupInvocationExpression = setupOrReturnInvocationExpression;
            }
            else if (setupOrReturnInvocationExpression.IsMoqReturnsMethod())
            {
                var invokedExpression2 = setupOrReturnInvocationExpression.InvokedExpression as IReferenceExpression;
                if (invokedExpression2 == null)
                    return;
                setupInvocationExpression = invokedExpression2.QualifierExpression as IInvocationExpression;
            }
            else
            {
                return;
            }
            var targetMethod = setupInvocationExpression.GetMockedMethodFromSetupMethod();
            if (targetMethod == null)
                return;
            string argsString = string.Join(", ", targetMethod.Parameters.Select(x => x.Type.GetPresentableName(CSharpLanguage.Instance) + " " + x.ShortName));
            var textualLookupItem = context.LookupItemsFactory.CreateTextLookupItem("(" + argsString + ") => {}");
            collector.AddToTop(textualLookupItem);
        }
    }
}