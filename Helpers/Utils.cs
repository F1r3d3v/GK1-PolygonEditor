namespace GK1_PolygonEditor
{
    internal static class Utils
    {
        public static void AddRadioCheckedBinding<T>(RadioButton radio, object dataSource, string dataMember, T trueValue)
        {
            var binding = new Binding(nameof(RadioButton.Checked), dataSource, dataMember, true, DataSourceUpdateMode.OnPropertyChanged);
            binding.Parse += (s, a) => { if ((bool)a.Value!) a.Value = trueValue; };
            binding.Format += (s, a) => a.Value = ((T)a.Value!).Equals(trueValue);
            radio.DataBindings.Add(binding);
        }
    }
}
