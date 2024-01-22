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
        if (data.WindSpeed is not null)
        {
            ProcessWind(message, data.WindSpeed.Value, data.WindGust);
        }
        if (data.Humidity is not null)
        {
            ProcessHumidity(message, data.Humidity.Value);
        }
        if (data.Pressure is not null)
        {
            ProcessPressure(message, data.Pressure.Value);
        }

        if (message.Length > 0)
        {
            return new WeatherEvaluatorData(true, message.ToString());
        }

        return new WeatherEvaluatorData(false, null);
    }

    private static void ProcessTemperatureAir(StringBuilder message, int value)
    {
        StringBuilder tempInfo = new();
        if (value >= 1 && value <= 2)
        {
            tempInfo.Append("На улице слякоть.");
        }
        else if (value <= -7)
        {
            if (value <= -15)
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
            tempInfo.Append($" Температура воздуха {value} °C");
            message.AppendLine(tempInfo.ToString());
        }
    }

    private static void ProcessPressure(StringBuilder message, int value)
    {
        const int normalPressure = 745;
        const int dispersion = 7;
        StringBuilder tempInfo = new();

        if (value >= normalPressure + dispersion)
        {
            tempInfo.Append($"Повышенное атмосферное давление {value} мм.р.с.");
        }
        if (value <= normalPressure - dispersion)
        {
            tempInfo.Append($"Пониженное атмосферное давление {value} мм.р.с.");
        }

        if (tempInfo.Length > 0)
        {
            message.AppendLine(tempInfo.ToString());
        }
    }

    private static void ProcessWind(StringBuilder message, int windSpeed, int? windGust)
    {
        StringBuilder tempInfo = new();
        const int bound1 = 4;
        const int bound2 = 7;
        const int bound3 = 10;
        var value = windSpeed;

        if (value >= bound1 && value < bound2)
        {
            tempInfo.Append($"Ощутимый ветер, скорость {value} м.с.");
        }
        else if (value >= bound2 && value < bound3)
        {
            tempInfo.Append($"Сильный ветер, скорость {value} м.с.");
        }
        else if (value >= bound3)
        {
            tempInfo.Append($"Очень cильный ветер, скорость {value} м.с.");
        }

        if (tempInfo.Length > 0 && windGust is not null)
        {
            tempInfo.Append($" с порывами {windGust.Value} м.с.");
        }

        if (tempInfo.Length > 0)
        {
            message.AppendLine(tempInfo.ToString());
        }
    }

    private static void ProcessHumidity(StringBuilder message, int value)
    {
        StringBuilder tempInfo = new();
        const int bound1 = 40;
        const int bound2 = 80;
        const int bound3 = 90;

        if (value <= bound1)
        {
            tempInfo.Append($"Низкая влажность воздуха {value}%");
        }
        else if (value >= bound2 && value < bound3)
        {
            tempInfo.Append($"Высокая влажность воздуха {value}%");
        }
        else if (value >= bound3)
        {
            tempInfo.Append($"Очень высокая влажность воздуха {value}%");
        }

        if (tempInfo.Length > 0)
        {
            message.AppendLine(tempInfo.ToString());
        }
    }

}
