using System;
using System.Collections.Generic;
using System.Data.Objects.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Wombit.EfRest
{
    public class ExpressionReWriterVisitor : ExpressionVisitor
    {
        private string[] expand;
        private string[] fields;
        private Stack<string> stack = new Stack<string>();
        private Stack<string> properties = new Stack<string>();

        public ExpressionReWriterVisitor(string[] fields, string[] expand)
        {
            this.fields = fields;
            this.expand = expand;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var expandSelect = typeof(LinqExpressions).GetMethods().Where(m => m.Name == "ExpandSelect").Skip(1).First();

            if (node.Method.IsGenericMethod && node.Method.Name == "ExpandSelect")
            {
                var enumerableSelectGeneric = typeof(Enumerable).GetMethods().Where(m => m.Name == "Select").First();

                var genericArgs = node.Method.GetGenericArguments();
                var enumerableSelect = enumerableSelectGeneric.MakeGenericMethod(genericArgs[0], typeof(object));
                var selectMethod = node.Arguments[1] as MethodCallExpression;
                var selectField = node.Arguments[1] as MemberExpression;


                Expression expression = null;

                if (selectMethod != null)
                {
                    var target = (selectMethod.Object as ConstantExpression).Value;
                    expression = (Expression)selectMethod.Method.Invoke(target, new object[] { }); //Selectas(method())
                }
                else
                {
                    var foo = (selectField.Expression as ConstantExpression).Value; //SelectAs(field)
                    var name = selectField.Member.Name;
                    expression = foo.GetType().GetField(name).GetValue(foo) as Expression;
                }
                expression = this.Visit(expression);

                var res = Expression.Call(enumerableSelect, node.Arguments[0], expression);
                return res;
            }
            if (node.Method.Name == "Select")
            {
                stack.Push("");
                var res = base.VisitMethodCall(node);
                if (stack.Count == 1)
                {
                    var source = node.Arguments[0];
                    var projection = node.Arguments[1] as UnaryExpression;
                    var lambda = (projection.Operand as LambdaExpression);
                    var body = lambda.Body;
                    var newe = body as NewExpression;
                    if (newe == null) //TODO: why is this select sometimes visited twice?
                        return node;

                    var init = ReplaceNewExpression(newe);
                    var lambdaType = projection.Type.GetGenericArguments()[0];
                    var lambda2 = Expression.Lambda(lambdaType, init, lambda.Parameters);
                    var projection2 = Expression.MakeUnary(ExpressionType.Quote, lambda2, projection.Type);

                    return Expression.Call(node.Method, source, projection2);
                    //we are at root level.
                    //remove properties not in fields

                }
                stack.Pop();
                return res;
            }

            return base.VisitMethodCall(node);
        }

        private MemberInitExpression ReplaceNewExpression(NewExpression newe)
        {
            var ctor = newe.Constructor;
            var argValues = newe.Arguments.ToArray();
            var argNames = ctor.GetParameters().Select(p => p.Name).ToArray();

            var template = new List<Tuple<string, Type>>();
            for (int i = 0; i < argNames.Length; i++)
            {
                var argName = argNames[i];
                var fullArgName = FullArgName(argName);

                if (ArgIncluded(argName))
                {
                    if (argValues[i].NodeType == ExpressionType.New)
                    {
                        var t = Tuple.Create(argNames[i], typeof(object));
                        template.Add(t);
                    }
                    else
                    {
                        var t = Tuple.Create(argNames[i], argValues[i].Type);
                        template.Add(t);
                    }
                }
            }
            var type = TypeGenerator.CreateType(template);
            var newe2 = Expression.New(type.GetConstructor(new Type[] { }));

            var bindings = new List<MemberBinding>();
            for (int i = 0; i < argNames.Length; i++)
            {
                var argName = argNames[i];
                var fullArgName = FullArgName(argName);

                if (ArgIncluded(argName))
                {
                    properties.Push(fullArgName);

                    var field = type.GetField(argNames[i]);

                    var value = this.Visit(argValues[i]);
                    var binding = Expression.Bind(field, value);
                    bindings.Add(binding);

                    properties.Pop();
                }
            }


            var init = Expression.MemberInit(newe2, bindings);
            return init;
        }

        private bool ArgIncluded(string argName)
        {
            var fullArgName = FullArgName(argName);
            return (argName == "href") || ((fields.Length == 0 || fields.Contains(fullArgName)) && (expand.Contains(CurrentProperty) || CurrentProperty == "" || (expand.FirstOrDefault() == "*")));
        }

        private string CurrentProperty
        {
            get
            {
                return properties.Count > 0 ? properties.Peek() : "";
            }
        }

        private string FullArgName(string argName)
        {
            return properties.Count > 0 ? properties.Peek() + "." + argName : argName;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            return base.VisitBinary(node);
        }

        public override Expression Visit(Expression node)
        {
            return base.Visit(node);
        }

        protected override Expression VisitBlock(BlockExpression node)
        {
            return base.VisitBlock(node);
        }

        protected override CatchBlock VisitCatchBlock(CatchBlock node)
        {
            return base.VisitCatchBlock(node);
        }

        protected override Expression VisitConditional(ConditionalExpression node)
        {
            return base.VisitConditional(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            return base.VisitConstant(node);
        }

        protected override Expression VisitDebugInfo(DebugInfoExpression node)
        {
            return base.VisitDebugInfo(node);
        }

        protected override Expression VisitDefault(DefaultExpression node)
        {
            return base.VisitDefault(node);
        }

        protected override Expression VisitDynamic(DynamicExpression node)
        {
            return base.VisitDynamic(node);
        }

        protected override ElementInit VisitElementInit(ElementInit node)
        {
            return base.VisitElementInit(node);
        }

        protected override Expression VisitExtension(Expression node)
        {
            return base.VisitExtension(node);
        }

        protected override Expression VisitGoto(GotoExpression node)
        {
            return base.VisitGoto(node);
        }

        protected override Expression VisitIndex(IndexExpression node)
        {
            return base.VisitIndex(node);
        }

        protected override Expression VisitInvocation(InvocationExpression node)
        {
            return base.VisitInvocation(node);
        }

        protected override Expression VisitLabel(LabelExpression node)
        {
            return base.VisitLabel(node);
        }

        protected override LabelTarget VisitLabelTarget(LabelTarget node)
        {
            return base.VisitLabelTarget(node);
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            return base.VisitLambda<T>(node);
        }

        protected override Expression VisitListInit(ListInitExpression node)
        {
            return base.VisitListInit(node);
        }

        protected override Expression VisitLoop(LoopExpression node)
        {
            return base.VisitLoop(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            return base.VisitMember(node);
        }

        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {
            return base.VisitMemberAssignment(node);
        }

        protected override MemberBinding VisitMemberBinding(MemberBinding node)
        {
            return base.VisitMemberBinding(node);
        }

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            return base.VisitMemberInit(node);
        }

        protected override MemberListBinding VisitMemberListBinding(MemberListBinding node)
        {
            return base.VisitMemberListBinding(node);
        }

        protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node)
        {
            return base.VisitMemberMemberBinding(node);
        }

        protected override Expression VisitNew(NewExpression node)
        {
            if (properties.Count > 0)
                return ReplaceNewExpression(node);
            //
            return base.VisitNew(node);
        }

        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            return base.VisitNewArray(node);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return base.VisitParameter(node);
        }

        protected override Expression VisitRuntimeVariables(RuntimeVariablesExpression node)
        {
            return base.VisitRuntimeVariables(node);
        }

        protected override Expression VisitSwitch(SwitchExpression node)
        {
            return base.VisitSwitch(node);
        }

        protected override SwitchCase VisitSwitchCase(SwitchCase node)
        {
            return base.VisitSwitchCase(node);
        }

        protected override Expression VisitTry(TryExpression node)
        {
            return base.VisitTry(node);
        }

        protected override Expression VisitTypeBinary(TypeBinaryExpression node)
        {
            return base.VisitTypeBinary(node);
        }
        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node.NodeType == ExpressionType.Quote)
            {

            }

            if (node.NodeType == ExpressionType.Convert && node.Operand.Type == typeof(int) && node.Type == typeof(object))
            {
                var operand = node.Operand;
                var stringConvertMethod = typeof(SqlFunctions).GetMethod("StringConvert", new Type[] { typeof(double?) });
                var trimMethod = typeof(string).GetMethod("Trim", new Type[] { });

                var dOperand = Expression.Convert(operand, typeof(double?));
                return Expression.Call(Expression.Call(stringConvertMethod, dOperand), trimMethod);
                //Expression<Func<int,string>> conv = i => SqlFunctions.StringConvert((double?)i).Trim();

            }

            return base.VisitUnary(node);
        }
    }
}
