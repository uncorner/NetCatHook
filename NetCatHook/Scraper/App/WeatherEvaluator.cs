using NetCatHook.Scraper.App.Parsing;
using System.Text;

namespace NetCatHook.Scraper.App;

record class WeatherEvaluatorData(bool Processed, string? TextMessage);

static class WeatherEvaluator
{
    
    public static WeatherEvaluatorData Evaluate(WeatherData data)
    {
        if (!data.Processed)
        {
            return new WeatherEvaluatorData(false, null);
        }

        StringBuilder message = new();
        if (data.TemperatureAir is not null)
        {
            ProcessTemperatureAir(message, data.TemperatureAir.Value);
        }

        if (message.Length > 0)
        {
            return new WeatherEvaluatorData(true, message.ToString());
        }

        return new WeatherEvaluatorData(false, null);
    }

    private static void ProcessTemperatureAir(StringBuilder message, int temperatureAir)
    {
        StringBuilder tempInfo = new();
        if (temperatureAir <= -7)
        {
            if (temperatureAir <= -15)
            {
                tempInfo.Append("На улице очень холодно, сильный мороз.");
            }
            else
            {
                tempInfo.Append("На улице холодно, мороз.");
            }
        }
        if (tempInfo.Length > 0)
        {
            tempInfo.Append($" Температура воздуха {temperatureAir} гр.ц.");
            message.AppendLine(tempInfo.ToString());
        }
    }


}
