using System.ServiceProcess;

namespace BBBSysTrayManager.Helper
{
    internal static class ServiceControllerTools
    {
        public static string ToStatusString(this ServiceControllerStatus status)
        {
            switch (status)
            {
                case ServiceControllerStatus.Running:
                    return "Iniciado";
                case ServiceControllerStatus.Stopped:
                    return "Parado";
                case ServiceControllerStatus.StartPending:
                    return "Iniciando";
                case ServiceControllerStatus.StopPending:
                    return "Parando";
                default: return "Desconhecido";
            }
        }
    }
}
