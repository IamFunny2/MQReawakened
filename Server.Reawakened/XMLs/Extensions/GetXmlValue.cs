﻿using Microsoft.Extensions.Logging;

namespace Server.Reawakened.XMLs.Extensions;

public static class GetXmlValue
{
    public static T GetEnumValue<T>(this T value, string attribute, Microsoft.Extensions.Logging.ILogger logger)
    {
        attribute = attribute.Replace(" ", string.Empty);

        if (attribute.Equals("invalid", StringComparison.CurrentCultureIgnoreCase))
            return value;
        else if (Enum.IsDefined(typeof(T), attribute))
            return (T)Enum.Parse(typeof(T), attribute, ignoreCase: true);

        logger.LogError("Unknown enum type: {Type}", attribute);

        return value;
    }

    public static bool GetBoolValue(this bool value, string attribute, Microsoft.Extensions.Logging.ILogger logger)
    {
        if (attribute.Equals("true", StringComparison.CurrentCultureIgnoreCase))
            return true;
        else if (attribute.Equals("false", StringComparison.CurrentCultureIgnoreCase))
            return false;

        logger.LogError("Unknown bool value: {Type}", attribute);

        return value;
    }

    public static DateTime GetDateValue(this DateTime value, string attribute, Microsoft.Extensions.Logging.ILogger logger)
    {
        if (attribute.Equals("none", StringComparison.CurrentCultureIgnoreCase))
            return value;

        if(DateTime.TryParse(attribute, out var date))
            return DateTime.SpecifyKind(date, DateTimeKind.Utc);

        logger.LogError("Unknown date time: {Type}", attribute);

        return value;
    }
}