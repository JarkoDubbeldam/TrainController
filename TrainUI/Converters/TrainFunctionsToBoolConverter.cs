//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Globalization;
//using System.Text;

//using Avalonia;
//using Avalonia.Controls;
//using Avalonia.Data;
//using Avalonia.Data.Converters;
//using Avalonia.Markup.Xaml;

//using Z21.Domain;

//namespace TrainUI.Converters {
//  public class TrainFunctionsToBoolConverter : IMultiValueConverter {
//    //public static readonly DirectProperty<TrainFunctionsToBoolConverter, TrainFunctions> MaskProperty =
//    //  AvaloniaProperty.RegisterDirect<TrainFunctionsToBoolConverter, TrainFunctions>(
//    //    nameof(Mask),
//    //    m => m.Mask,
//    //    (c, m) => c.Mask = m);

//    public TrainFunctions Mask { get; set; }

//    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
//      var trainFunctions = (TrainFunctions)value;
//      var mask = (TrainFunctions)parameter;
//      var interestingFunctions = (trainFunctions & mask);
//      if (interestingFunctions == mask) {
//        return true;
//      } else if (interestingFunctions == TrainFunctions.None) {
//        return false;
//      } else {
//        return null;
//      }
//    }

//    public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture) {
//      throw new NotImplementedException();
//    }

//    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
//      throw new NotImplementedException();
//    }
//  }


//  public class ConverterBindableParameter : MarkupExtension {
//    #region Public Properties

//    public Binding Binding { get; set; }
//    public BindingMode Mode { get; set; }
//    public IValueConverter Converter { get; set; }
//    public Binding ConverterParameter { get; set; }

//    #endregion

//    public ConverterBindableParameter() { }

//    public ConverterBindableParameter(string path) {
//      Binding = new Binding(path);
//    }

//    public ConverterBindableParameter(Binding binding) {
//      Binding = binding;
//    }

//    #region Overridden Methods

//    public override object ProvideValue(IServiceProvider serviceProvider) {
//      var multiBinding = new MultiBinding();
//      Binding.Mode = Mode;
//      multiBinding.Bindings.Add(Binding);
//      if (ConverterParameter != null) {
//        ConverterParameter.Mode = BindingMode.OneWay;
//        multiBinding.Bindings.Add(ConverterParameter);
//      }
//      var adapter = new MultiValueConverterAdapter {
//        Converter = Converter
//      };
//      multiBinding.Converter = adapter;
//      return multiBinding.ProvideValue(serviceProvider);
//    }

//    #endregion

//    [ContentProperty(nameof(Converter))]
//    private class MultiValueConverterAdapter : IMultiValueConverter {
//      public IValueConverter Converter { get; set; }

//      private object lastParameter;

//      public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture) {
//        if (Converter == null) return values[0]; // Required for VS design-time
//        if (values.Count > 1) lastParameter = values[1];
//        return Converter.Convert(values[0], targetType, lastParameter, culture);
//      }

//      public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
//        if (Converter == null) return new object[] { value }; // Required for VS design-time

//        return new object[] { Converter.ConvertBack(value, targetTypes[0], lastParameter, culture) };
//      }
//    }
//  }
//}

