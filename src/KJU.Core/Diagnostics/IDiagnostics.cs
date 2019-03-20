namespace KJU.Core.Diagnostics
{
    public interface IDiagnostics
    {
        void Add(params Diagnostic[] diagnostics);

        /**
         * <summary>
         * In case of errors turns on fire alarm.
         * </summary>
         */
        void Report();
    }
}