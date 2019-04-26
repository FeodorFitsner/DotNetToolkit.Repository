﻿namespace DotNetToolkit.Repository.Configuration.Logging
{
    using JetBrains.Annotations;
    using System;
    using Utility;

    /// <summary>
    /// Contains various extension methods for <see cref="ILogger" />.
    /// </summary>
    public static class LoggerExtensions
    {
        /// <summary>
        /// Logs a message with a <see cref="LogLevel.Debug" /> severity.
        /// </summary>
        /// <param name="source">The logger.</param>
        /// <param name="message">The message to log.</param>
        public static void Debug([NotNull] this ILogger source, [NotNull] string message)
        {
            Guard.NotNull(source);
            Guard.NotNull(message);

            source.Log(LogLevel.Debug, message);
        }

        /// <summary>
        /// Logs a composite format message with a <see cref="LogLevel.Debug" /> severity.
        /// </summary>
        /// <param name="source">The logger.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Debug([NotNull] this ILogger source, [NotNull] string message, [CanBeNull] params object[] args)
        {
            Guard.NotNull(source);
            Guard.NotNull(message);

            Debug(source, string.Format(message, args));
        }

        /// <summary>
        /// Logs a message with a <see cref="LogLevel.Info" /> severity.
        /// </summary>
        /// <param name="source">The logger.</param>
        /// <param name="message">The message to log.</param>
        public static void Info([NotNull] this ILogger source, [NotNull] string message)
        {
            Guard.NotNull(source);
            Guard.NotNull(message);

            source.Log(LogLevel.Info, message);
        }

        /// <summary>
        /// Logs a composite format message with a <see cref="LogLevel.Info" /> severity.
        /// </summary>
        /// <param name="source">The logger.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Info([NotNull] this ILogger source, [NotNull] string message, [CanBeNull] params object[] args)
        {
            Guard.NotNull(source);
            Guard.NotNull(message);

            Info(source, string.Format(message, args));
        }

        /// <summary>
        /// Logs a message with a <see cref="LogLevel.Warning" /> severity.
        /// </summary>
        /// <param name="source">The logger.</param>
        /// <param name="message">The message to log.</param>
        public static void Warning([NotNull] this ILogger source, [NotNull] string message)
        {
            Guard.NotNull(source);
            Guard.NotNull(message);

            source.Log(LogLevel.Warning, message);
        }

        /// <summary>
        /// Logs a composite format message with a <see cref="LogLevel.Warning" /> severity.
        /// </summary>
        /// <param name="source">The logger.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Warning([NotNull] this ILogger source, [NotNull] string message, [CanBeNull] params object[] args)
        {
            Guard.NotNull(source);
            Guard.NotNull(message);

            Warning(source, string.Format(message, args));
        }

        /// <summary>
        /// Logs a message with a <see cref="LogLevel.Error" /> severity.
        /// </summary>
        /// <param name="source">The logger.</param>
        /// <param name="message">The message to log.</param>
        public static void Error([NotNull] this ILogger source, [NotNull] string message)
        {
            Guard.NotNull(source);
            Guard.NotNull(message);

            source.Log(LogLevel.Error, message);
        }

        /// <summary>
        /// Logs a composite format message with a <see cref="LogLevel.Error" /> severity.
        /// </summary>
        /// <param name="source">The logger.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public static void Error([NotNull] this ILogger source, [NotNull] string message, [CanBeNull] params object[] args)
        {
            Guard.NotNull(source);
            Guard.NotNull(message);

            Error(source, string.Format(message, args));
        }

        /// <summary>
        /// Logs a message associated with an exception with a <see cref="LogLevel.Error" /> severity.
        /// </summary>
        /// <param name="source">The logger.</param>
        /// <param name="exception">The associated exception to log.</param>
        /// <param name="message">The message to log.</param>
        public static void Error([NotNull] this ILogger source, [NotNull] Exception exception, [NotNull] string message)
        {
            Guard.NotNull(source);
            Guard.NotNull(exception);
            Guard.NotNull(message);

            Error(source, $"{message}{Environment.NewLine}{exception}");
        }

        /// <summary>
        /// Logs an exception with a <see cref="LogLevel.Error" /> severity.
        /// </summary>
        /// <param name="source">The logger.</param>
        /// <param name="exception">The associated exception to log.</param>
        public static void Error([NotNull] this ILogger source, [NotNull] Exception exception)
        {
            Guard.NotNull(source);

            Error(source, exception.ToString());
        }
    }
}
