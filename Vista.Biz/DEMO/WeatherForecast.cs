using System;

namespace Vista.Biz.DEMO;

internal record WeatherForecast
{
    public int Sn { get; set; }
    public DateTime Date { get; set; }
    public string City { get; set; } = string.Empty;
    public int TemperatureC { get; set; }
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    public string Summary { get; set; } = string.Empty;
}

internal record QryForecastArgs
{
    public DateTime? startDate { get; set; } = DateTime.Today;
    public decimal rowCount { get; set; } = 80;
    public bool simsFail { get; set; } = false;
}
