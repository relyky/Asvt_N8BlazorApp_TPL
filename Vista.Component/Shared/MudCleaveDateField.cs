using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Globalization;
using Vista.Component.Services;

namespace Vista.Component.Shared;

/// <summary>
/// To pick date with MudDatePicker & Cleave.js mask 
/// mask format: YYYY/MM/DD
/// </summary>
public class MudCleaveDateField : MudDatePicker
{
  [Inject] protected DateMaskJsInterop jsr { get; set; } = default!;

  public MudCleaveDateField()
  {
    UserAttributes?.Add("data-muddate", "true");
    DateFormat = "yyyy/MM/dd";
    Editable = true;
    //ImmediateText = true;
    TitleDateFormat = "yyyy年M月d日";
  }

  protected override async Task OnAfterRenderAsync(bool firstRender)
  {
    if (firstRender)
    {
      var cleaveOptions = new
      {
        date = true,
        datePattern = new[] { "Y", "m", "d" },
        delimiter = '/'
      };

      var inputElement = this._inputReference.InputReference.ElementReference; 
      await jsr.SetCleaveMaskAsync(inputElement, cleaveOptions);
    }

    await base.OnAfterRenderAsync(firstRender);
  }

  protected override Task StringValueChangedAsync(string value)
  {
    if (!string.IsNullOrEmpty(value))
    {
      if (DateTime.TryParseExact(value, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime validDate))
      {
        Date = validDate;
      }
    }

    return base.StringValueChangedAsync(value);
  }
}
