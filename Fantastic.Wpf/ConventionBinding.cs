using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Shapes;
using System.Windows.Threading;
using Fantastic.Utils;

namespace Fantastic.Wpf
{
    using static EnumerableUtils;
    using Primitives = System.Windows.Controls.Primitives;

    public class ConventionBinding<TController, TModel, TView> where TController : IViewController<TModel>
                                                               where TView : DependencyObject
    {
        static readonly Dictionary<Tuple<Type, string>, PropertyInfo> properties = new Dictionary<Tuple<Type, string>, PropertyInfo>();

        static readonly Dictionary<PropertyInfo, Type> boundPropertyType = new Dictionary<PropertyInfo, Type>();

        static readonly Dictionary<string, Action<TController, object>> actions = new Dictionary<string, Action<TController, object>>();

        static readonly Dictionary<string, Func<TController, object, bool>> actionsCanExecute = new Dictionary<string, Func<TController, object, bool>>();

        static readonly Dictionary<Type, DependencyProperty> conventions = new Dictionary<Type, DependencyProperty>();

        static readonly Dictionary<Type, Action<FrameworkElement, bool>> readOnlySettings = new Dictionary<Type, Action<FrameworkElement, bool>>();

        static readonly Dictionary<Type, Func<object, PropertyInfo, object>> propertyICreator = new Dictionary<Type, Func<object, PropertyInfo, object>>();

        static readonly Dictionary<Type, Func<object, PropertyInfo, DispatcherTimer, object>> propertyOCreator =
            new Dictionary<Type, Func<object, PropertyInfo, DispatcherTimer, object>>();

        static readonly Dictionary<Type, Func<object, PropertyInfo, object>> propertyCollectionCreator = new Dictionary<Type, Func<object, PropertyInfo, object>>();

        static readonly IValueConverter booleanToVisibilityConverter = new BooleanToVisibilityConverter();

        //static readonly Func<string, string> singularize = original => {
        //    return original.EndsWith("ies", StringComparison.OrdinalIgnoreCase)
        //        ? original.TrimEnd('s').TrimEnd('e').TrimEnd('i') + "y"
        //        : original.EndsWith("ches", StringComparison.OrdinalIgnoreCase) ?
        //        original.TrimEnd(new[] { 'e', 's' })
        //        : original.TrimEnd('s');
        //};

        static readonly Func<string, string> pluralize = original => {
            return original.EndsWith('y')
                ? original.TrimEnd('y') + "ies"
                : original.EndsWith("ch", StringComparison.OrdinalIgnoreCase) ?
                  original + "es"
                : original + "s";
        };

        static ConventionBinding()
        {

            typeof(TController).GetProperties()
                     .Select(a => Tuple.Create(typeof(TController), a))
                     .Concat(
            typeof(TModel).GetProperties()
                     .Select(a => Tuple.Create(typeof(TModel), a)))
                     .ForEach(a => properties.Add(Tuple.Create(a.Item1, a.Item2.Name), a.Item2));


            properties.Select(a => a.Value)
                      .ForEach(a => boundPropertyType[a] = a.PropertyType);


            typeof(TController).GetMethods()
                     .Where(a => a.GetParameters().Length == 0 && a.ReturnType.Equals(typeof(void)))
                     .Select(a => Tuple.Create(a, (Action<TController>)Delegate.CreateDelegate(typeof(Action<TController>), a, true)))
                     .ForEach(a => actions.Add(a.Item1.Name, (b, c) => a.Item2(b)));

            typeof(TController).GetMethods()
                     .Where(a => a.GetParameters().Length == 1 && a.GetParameters()[0].ParameterType.Equals(typeof(object)) && a.ReturnType.Equals(typeof(void)))
                     .ForEach(a => actions.Add(a.Name, (Action<TController, object>)Delegate.CreateDelegate(typeof(Action<TController, object>), a, true)));

            var canExecutePrefix = "Can";

            actions.Select(a => typeof(TController).GetMethod(canExecutePrefix + a.Key))
                   .Where(a => a != null && a.GetParameters()
                                             .Select((b, i) => new { b.ParameterType, i })
                                             .Where(b => b.i == 0 && b.ParameterType == typeof(object))
                                             .Count() == 1)
                   .ForEach(
                        a => {
                            actionsCanExecute.Add(a.Name.Substring(3),
                                                  (Func<TController, object, bool>)Delegate.CreateDelegate(typeof(Func<TController, object, bool>), a));

                        }
                   );

            new Tuple<Type, DependencyProperty>[]{
                Tuple.Create(typeof(Calendar), Calendar.SelectedDateProperty),
                Tuple.Create(typeof(DatePicker), DatePicker.SelectedDateProperty),
                Tuple.Create(typeof(DataGrid), DataGrid.ItemsSourceProperty),
                Tuple.Create(typeof(UserControl),UserControl.VisibilityProperty),
                Tuple.Create(typeof(Image),Image.SourceProperty),
                Tuple.Create(typeof(Primitives.ToggleButton),Primitives.ToggleButton.IsCheckedProperty),
                Tuple.Create(typeof(Primitives.ButtonBase),Primitives.ButtonBase.ContentProperty),
                Tuple.Create(typeof(TextBox),TextBox.TextProperty),
                Tuple.Create(typeof(TextBlock),TextBlock.TextProperty),
                Tuple.Create(typeof(ProgressBar),ProgressBar.ValueProperty),
                Tuple.Create(typeof(Primitives.Selector),Primitives.Selector.SelectedItemProperty),
                Tuple.Create(typeof(ItemsControl),ItemsControl.ItemsSourceProperty),
                Tuple.Create(typeof(ContentControl),ContentControl.ContentProperty),
                Tuple.Create(typeof(Shape),Shape.VisibilityProperty),
                Tuple.Create(typeof(FrameworkElement),FrameworkElement.VisibilityProperty)
            }.ForEach(a => conventions.Add(a.Item1, a.Item2));

            new Tuple<Type, Action<FrameworkElement, bool>>[]{
                Tuple.Create<Type, Action<FrameworkElement, bool>>(typeof(Calendar),
                                                                   (a, b) => { if(HasBinding(a, Calendar.IsEnabledProperty)){return;}
                                                                               (a as Calendar).IsEnabled = !b; }),
                Tuple.Create<Type, Action<FrameworkElement, bool>>(typeof(DatePicker),
                                                                   (a, b) => { if(HasBinding(a, DatePicker.IsEnabledProperty)){return;}
                                                                               (a as DatePicker).IsEnabled = !b; }),
                Tuple.Create<Type, Action<FrameworkElement, bool>>(typeof(DataGrid),
                                                                   (a, b) => { if(HasBinding(a, DataGrid.IsReadOnlyProperty)){return;}
                                                                               (a as DataGrid).IsReadOnly = b; }),
                Tuple.Create<Type, Action<FrameworkElement, bool>>(typeof(Primitives.ToggleButton),
                                                                   (a, b) => {if(HasBinding(a, Primitives.ToggleButton.IsEnabledProperty)){return;}
                                                                              (a as Primitives.ToggleButton).IsEnabled = !b; }),
                Tuple.Create<Type, Action<FrameworkElement, bool>>(typeof(Primitives.ButtonBase),
                                                                   (a, b) => {if(HasBinding(a, Primitives.ButtonBase.IsEnabledProperty)){return;}
                                                                              (a as Primitives.ButtonBase).IsEnabled = !b; }),
                Tuple.Create<Type, Action<FrameworkElement, bool>>(typeof(TextBox),
                                                                   (a, b) => {if(HasBinding(a, TextBox.IsReadOnlyProperty)){return;}
                                                                              (a as TextBox).IsReadOnly = b; }),
                Tuple.Create<Type, Action<FrameworkElement, bool>>(typeof(Primitives.Selector),
                                                                   (a, b) => { if(HasBinding(a, Primitives.Selector.IsEnabledProperty)){return;}
                                                                               (a as Primitives.Selector).IsEnabled = !b; }),
            }.ForEach(a => readOnlySettings.Add(a.Item1, a.Item2));
        }

        readonly DispatcherTimer timer = new DispatcherTimer();

        public ConventionBinding()
        {
            timer.Interval = TimeSpan.FromSeconds(1 / 60);

            timer.Start();
        }

        static void GetLogicalChildCollection<W>(DependencyObject parent, List<W> logicalCollection) where W : DependencyObject
        {
            var children = LogicalTreeHelper.GetChildren(parent);
            foreach (object child in children)
            {
                if (child is DependencyObject)
                {
                    var dependencyChild = child as DependencyObject;
                    if (child is W)
                    {
                        logicalCollection.Add(child as W);
                    }
                    GetLogicalChildCollection(dependencyChild, logicalCollection);
                }
            }
        }

        static IList<FrameworkElement> GetNamedElements(TView view)
        {

            var elements = new List<FrameworkElement>();

            GetLogicalChildCollection(view, elements);

            return elements.Where(a => !string.IsNullOrWhiteSpace(a.Name)).ToArray();
        }

        static Type GetPropertyTypeEraseNullable(PropertyInfo property)
        {
            var type = boundPropertyType[property];

            const string nullable = "Nullable`1";

            return type.IsGenericType && type.Name.StartsWith(nullable, StringComparison.Ordinal) ? type.GetGenericArguments()[0] : type;
        }

        IList<FrameworkElement> BindProperties(TController controller, IList<FrameworkElement> namedElements)
        {

            var unmatchedElements = new List<FrameworkElement>();

            foreach (var element in namedElements)
            {

                if (!BindProperty(element, controller))
                {
                    unmatchedElements.Add(element);
                }

                BindPropertiesByConvention(element, controller);

                ApplyReadOnly(controller, element);

                ApplySelectOnFocus(element);

            }


            return unmatchedElements;
        }

        IList<FrameworkElement> BindActions(TController controller, IList<Primitives.ButtonBase> namedElements)
        {

            var unmatchedElements = new List<FrameworkElement>();

            foreach (var element in namedElements)
            {

                if (!BindAction(element, controller))
                {
                    unmatchedElements.Add(element);
                }

                BindPropertiesByConvention(element, controller);

                ApplyReadOnly(controller, element);
            }

            return unmatchedElements;
        }

        bool BindProperty(FrameworkElement element, TController controller)
        {

            var cleanName = element.Name.Trim('_');

            var parts = cleanName.Split(new[] { '_', }, StringSplitOptions.RemoveEmptyEntries).ToList();

            var rootProperty =
                (element.DataContext != null ? element.DataContext.GetType().GetProperty(parts[0]) : null) ??
                           (properties.ContainsKey(Tuple.Create(typeof(TModel), parts[0])) ? properties[Tuple.Create(typeof(TModel), parts[0])] :
                            properties.ContainsKey(Tuple.Create(typeof(TController), parts[0])) ? properties[Tuple.Create(typeof(TController), parts[0])] : null);

            var property = rootProperty;

            for (int i = 1; i < parts.Count && property != null; i++)
            {
                property = property.PropertyType.GetProperty(parts[i]);
            }

            if (property == null)
            {
                Console.WriteLine("Binding Convention Not Applied: Element {0} did not match a property.", element.Name);
                return false;
            }

            var convention = GetValue(conventions, element.GetType());

            if (convention == null)
            {
                Console.WriteLine("Binding Convention Not Applied: No conventions configured for {0}.", element.GetType());
                return false;
            }

            if (!boundPropertyType.ContainsKey(property))
            {
                var b = property.PropertyType;

                boundPropertyType[property] = b;
            }

            var useViewedObject = properties.Any(b => b.Key.Item1 == typeof(TModel) && b.Value == rootProperty) ? "ViewedObject." : string.Empty;

            var applied =
                ApplyBinding(
                    useViewedObject + string.Join(".", parts),
                    property,
                    element,
                    convention,
                    controller
                );

            if (applied)
            {

                var propertyType = GetPropertyTypeEraseNullable(property);

                switch (Type.GetTypeCode(propertyType))
                {
                    case TypeCode.Decimal:
                    case TypeCode.Double:
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.Single:
                        if (element is TextBlock)
                        {
                            (element as TextBlock).TextAlignment = TextAlignment.Right;
                        }
                        else if (element is TextBox)
                        {
                            (element as TextBox).TextAlignment = TextAlignment.Right;
                        }
                        break;
                }
            }

            if (applied)
            {
                Console.WriteLine("Binding Convention Applied: Element {0}.", element.Name);
                return true;
            }


            Console.WriteLine("Binding Convention Not Applied: Element {0} has existing binding.", element.Name);

            return false;
        }

        void BindPropertiesByConvention(FrameworkElement element, TController controller)
        {
            var p = properties.Where(a => a.Key.Item1.Equals(typeof(TController)) &&
                                          a.Key.Item2.StartsWith(element.Name) &&
                                          !a.Key.Item2.Equals(element.Name))
                              .Select(a => Tuple.Create(a.Key.Item2.TrimStart(element.Name.ToCharArray()), a.Value))
                              .Select(a => Tuple.Create(element.GetType()
                                                               .GetField(a.Item1 + "Property",
                                                                         BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static),
                                                         a.Item2)
                                     )
                              .Where(a => a.Item1 != null && typeof(DependencyProperty).IsAssignableFrom(a.Item1.FieldType))
                              .SingleOrDefault();

            if (p != null)
            {
                var dependency = (DependencyProperty)p.Item1.GetValue(controller);
                ApplyBinding(p.Item2.Name, p.Item2, element, dependency, controller);
            }
        }

        static bool BindAction(Primitives.ButtonBase element, TController controller)
        {

            const string notApplied = "Binding Convention Not Applied: Element {0} is already bound to a command.";

            if (element.Command != null)
            {
                Console.WriteLine(notApplied, element.Name);
                return false;
            }

            var parts = element.Name.Trim('_').Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);

            var action = actions.ContainsKey(parts[0]) ? actions[parts[0]] : null;

            if (action == null)
            {
                Console.WriteLine(notApplied, element.Name);
                return false;
            }

            Predicate<object> canExecute = null;

            if (actionsCanExecute.ContainsKey(parts[0]))
            {
                canExecute = a => actionsCanExecute[parts[0]](controller, element.CommandParameter ?? element.DataContext);
            }

            var parameter = parts.Length == 2 ? parts[1] : null;

            var command = new RelayCommand(a => action(controller, parameter ?? element.CommandParameter ?? element.DataContext), canExecute);

            element.Command = command;

            return true;
        }

        static void ApplyBindingMode(Binding binding, PropertyInfo property)
        {
            binding.Mode =
                property.CanWrite && property.GetSetMethod(true).IsPublic ? BindingMode.TwoWay : BindingMode.OneWay;
        }

        static void ApplyValidation(Binding binding, PropertyInfo property)
        {
            if (typeof(IDataErrorInfo).IsAssignableFrom(typeof(TModel)))
            {
                binding.ValidatesOnDataErrors = true;
            }
        }

        static void ApplyValueConverter(Binding binding, DependencyProperty bindableProperty, PropertyInfo property)
        {

            if (bindableProperty == UIElement.VisibilityProperty &&
                typeof(bool).IsAssignableFrom(boundPropertyType[property]))
            {
                binding.Converter = booleanToVisibilityConverter;
            }

        }

        static void ApplyStringFormat(Binding binding, PropertyInfo property)
        {

            if (binding.StringFormat != null)
            {
                return;
            }

            // not handling Nullable<>
            switch (Type.GetTypeCode(GetPropertyTypeEraseNullable(property)))
            {
                case TypeCode.DateTime:
                    binding.StringFormat = "{0:MMMM dd yyyy hh:mm:ss tt}";
                    break;
                case TypeCode.Decimal:
                    binding.StringFormat = "N2";
                    break;
                case TypeCode.Int32:
                case TypeCode.Int64:
                    binding.StringFormat = "N0";
                    break;
            }
        }

        static void ApplyUpdateSourceTrigger(DependencyProperty dependencyProperty, DependencyObject element, Binding binding, PropertyInfo property)
        {

            var textBox = element as TextBox;

            if (textBox != null && dependencyProperty == TextBox.TextProperty)
            {
                if (property != null)
                {
                    var typeCode = Type.GetTypeCode(GetPropertyTypeEraseNullable(property));
                    if (typeCode == TypeCode.Single || typeCode == TypeCode.Double || typeCode == TypeCode.Decimal)
                    {
                        binding.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
                        textBox.KeyUp += (x, y) => {

                            var start = textBox.SelectionStart;
                            var text = textBox.Text;

                            var bindingExpression = textBox.GetBindingExpression(dependencyProperty);

                            if (bindingExpression != null)
                            {
                                bindingExpression.UpdateSource(); // exception can be thrown here
                            }

                            textBox.Text = text;
                            textBox.SelectionStart = start;
                        };

                        return;
                    }
                }

                textBox.TextChanged += (x, y) => { textBox.GetBindingExpression(dependencyProperty).UpdateSource(); };
                return;
            }

            if ((element as ComboBox) != null && dependencyProperty == Primitives.Selector.SelectedItemProperty)
            {
                binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            }

        }

        static void ApplyReadOnly(TController controller, DependencyObject element)
        {

            if (!(element is FrameworkElement))
            {
                return;
            }

            var f = element as FrameworkElement;

            GetValue(readOnlySettings, element.GetType())?.Invoke(f, controller.IsReadOnly);
        }

        bool ApplyBinding(string path, PropertyInfo property, FrameworkElement element, DependencyProperty convention, TController controller)
        {

            if (HasBinding(element, convention))
            {
                return false;
            }

            var propType = property.PropertyType;

            if (!propertyICreator.ContainsKey(propType))
            {
                var genType = typeof(PropertyI<>).MakeGenericType(propType);


                var constructor = genType.GetConstructors()
                                         .Single();

                propertyICreator[propType] = (a, b) => constructor.Invoke(new[] { a, b });
            }

            if (!propertyOCreator.ContainsKey(propType))
            {
                var genType = typeof(PropertyO<>).MakeGenericType(propType);

                var constructor = genType.GetConstructors()
                                         .Single();

                propertyOCreator[propType] = (a, b, c) => constructor.Invoke(new[] { a, b, c });
            }

            var genericArgument = propType.IsGenericType ? propType.GetGenericArguments()[0] : null;

            var isCollectionType = propType.IsGenericType &&
                                   typeof(ICollection<>)
                                        .MakeGenericType(genericArgument)
                                        .IsAssignableFrom(propType);

            if (isCollectionType && !propertyCollectionCreator.ContainsKey(propType))
            {
                var genType = typeof(PropertyCollection<,>).MakeGenericType(propType, genericArgument);

                var constructor = genType.GetConstructors()
                                         .Single();

                propertyCollectionCreator[propType] = (a, b) => constructor.Invoke(new[] { a, b , timer});
            }

            var propOwner = property.DeclaringType;

            object getSource()
            {
                var result = element.DataContext;

                var props = path.Split('.');

                for (int i = 0; i < props.Length - 1; i++)
                {
                    result = result.GetType()
                                   .GetProperty(props[i])
                                   .GetGetMethod()
                                   ?.Invoke(result, null);
                }

                return result;
            }

            object propSource =
                propOwner == element.DataContext.GetType() ? element.DataContext :
                propOwner == typeof(TController) ? controller as object :
                propOwner == typeof(TModel) ? controller.Model :
                element.DataContext != null ? getSource() :
                null;

            if (propSource == null)
            {
                Console.WriteLine($"Could not resolve path {path}");

                return false;
            }

            var prop =
                isCollectionType ? propertyCollectionCreator[propType](propSource, property) :
                property.CanWrite ? propertyICreator[propType](propSource, property) :
                                    propertyOCreator[propType](propSource, property, timer);

            var binding = new Binding("Value");

            element.DataContext = prop;

            ApplyBindingMode(binding, property);
            ApplyValueConverter(binding, convention, property);
            ApplyStringFormat(binding, property);
            ApplyValidation(binding, property);
            ApplyUpdateSourceTrigger(convention, element, binding, property);

            BindingOperations.SetBinding(element, convention, binding);

            if (element is Primitives.Selector)
            {
                ConfigureSelector(element as Primitives.Selector, property, controller);
            }

            return true;
        }

        static void ApplySelectOnFocus(FrameworkElement element)
        {
            var t = element as TextBox;

            if (t != null)
            {
                t.GotFocus += (_, __) => t.SelectAll();
            }
        }

        static bool HasBinding(FrameworkElement element, DependencyProperty property)
        {
            var bindingExpression = element.GetBindingExpression(property);

            return bindingExpression != null;
        }

        static void ConfigureSelector(Primitives.Selector selector, PropertyInfo property, TController controller)
        {

            var c = selector as ComboBox;

            if (c != null)
            {
                c.IsEditable = true;
            }

            if (selector.DisplayMemberPath == null && !HasBinding(selector, Primitives.Selector.DisplayMemberPathProperty))
            {
                var displayMember = "Name";

                selector.DisplayMemberPath = displayMember;
            }

            var itemsSourceProperty = ItemsControl.ItemsSourceProperty;

            if (!HasBinding(selector, itemsSourceProperty))
            {

                var binding = new Binding()
                {
                    Source = controller,
                    Path = new PropertyPath(pluralize(selector.Name)),
                    Mode = BindingMode.OneWay,
                };

                BindingOperations.SetBinding(selector, itemsSourceProperty, binding);
            }

            if (property.GetIndexParameters().Length > 0 || selector.ItemsSource == null || !selector.ItemsSource.OfType<object>().Any())
            {
                return;
            }

            object value = null;

            if (property.DeclaringType.IsAssignableFrom(typeof(TController)))
            {
                value = property.GetValue(controller, null);
            }
            else if (property.DeclaringType.IsAssignableFrom(typeof(TModel)))
            {
                value = property.GetValue(controller.Model, null);
            }
            else if (selector.DataContext != null && property.DeclaringType.IsAssignableFrom(selector.DataContext.GetType()))
            {
                value = property.GetValue(selector.DataContext, null);
            }
            else if (selector.DataContext != null && selector.DataContext is IValue)
            {
                value = ((IValue)selector.DataContext).Value;
            }

            var itemsSource = selector.ItemsSource
                                .Cast<object>()
                                .Select((a, i) => new { a, i, b = value })
                                .ToArray();

            var index = itemsSource.FirstOrDefault(a => a.a == value || (a.b != null && a.b == a.a));

            selector.SelectedIndex = index == null ? -1 : index.i;
        }

        static void ConfigureDataGrid(IList<DataGrid> grids)
        {

            grids.ForEach(
                a => {
                    //a.CanUserAddRows = true;
                    a.CanUserDeleteRows = true;
                    a.CanUserReorderColumns = true;
                    a.CanUserResizeColumns = true;
                    a.CanUserResizeRows = true;
                    a.CanUserSortColumns = true;
                    a.GridLinesVisibility = DataGridGridLinesVisibility.None;

                    a.MouseUp += (_, __) =>
                    {
                        a.BeginEdit();
                    };
                });

            grids.SelectMany(a => a.Columns)
                 .ForEach(
                    a => {

                        if (!string.IsNullOrWhiteSpace(a.SortMemberPath))
                        {
                            return;
                        }

                        var sortClipboard = (string)a.GetValue(SortingClipboard.SortAndClipboardPathProperty);

                        if (sortClipboard != null)
                        {
                            a.SortMemberPath = sortClipboard;
                        }
                        else if (a.Header != null && a.Header is string)
                        {
                            a.SortMemberPath = (a.Header as string).Replace(" ", string.Empty);
                        }

                        if (a.SortMemberPath != null)
                        {
                            a.ClipboardContentBinding = new Binding(a.SortMemberPath);
                        }

                    });

        }

        public IList<FrameworkElement> Bind(TView view, TController controller)
        {

            var namedElements = GetNamedElements(view);

            var unboundProperties = BindProperties(controller, namedElements);

            var unboundActions = BindActions(controller, namedElements.OfType<Primitives.ButtonBase>().ToArray());

            ConfigureDataGrid(namedElements.OfType<DataGrid>().ToList());

            return unboundProperties.Join(unboundActions, a => a, a => a, (a, b) => a).ToList();
        }

        public void WireUp(FrameworkElement element, TView view, TController controller)
        {

            var bindToProperty = true;

            if (element is Primitives.ButtonBase)
            {
                bindToProperty = !BindAction(element as Primitives.ButtonBase, controller);
            }

            if (bindToProperty)
            {
                BindProperty(element, controller);
            }

            ApplyReadOnly(controller, element);

            ApplySelectOnFocus(element);
        }

        static W GetValue<W>(Dictionary<Type, W> keyedByType, Type key) where W : class
        {

            if (key == null)
            {
                return null;
            }

            if (keyedByType.ContainsKey(key))
            {
                return keyedByType[key];
            }

            return GetValue(keyedByType, key.BaseType);
        }

    }

}
