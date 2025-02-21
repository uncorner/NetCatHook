using System.Text;
using NetCatHook.Scraper.Domain.Entities;

namespace NetCatHook.Scraper.Application;

static class WeatherSummaryBuilder
{

    public static string Build(WeatherReport report)
    {
        StringBuilder str = new();
        if (report.InCity is not null)
        {
            str.AppendFormat("Погода {0} на {1:dd.MM.yyyy H:mm}:",
                report.InCity,  report.CreatedAtLocal);
        }
        else
        {
            str.AppendFormat("Погода на {0:dd.MM.yyyy H:mm}:",
                report.CreatedAtLocal);
        }
        str.AppendLine();

        if (report.Description is not null)
        {
            str.AppendLine(report.Description);
        }

        if (report.TemperatureAir is not null)
        {
            str.AppendLine($"Температура воздуха {report.TemperatureAir} °C");
        }

        if (report.WindSpeed is not null)
        {
            str.Append($"Скорость ветра {report.WindSpeed} м.с.");
            if (report.WindGust is not null)
            {
                str.Append($" с порывами {report.WindGust} м.с.");
            }
            str.AppendLine();
        }

        if (report.Humidity is not null)
        {
            str.AppendLine($"Влажность воздуха {report.Humidity} %");
        }

        if (report.Pressure is not null) {
            str.AppendLine($"Атмосферное давление {report.Pressure} мм.р.с.");
        }

        return str.ToString();
    }

}
