// <copyright file="ConsoleWrapper.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Console
{
    public class ConsoleWrapper
    {
        private ConsoleInterface? consoleInterface;

        public void SetInterface(ConsoleInterface activeInterface)
        {
            consoleInterface = activeInterface;
        }

        public void ClearInterface()
        {
            consoleInterface = null;
        }

        public void WriteResponse(string response)
        {
            consoleInterface?.WriteResponse(response);
        }

        public void WriteError(string error)
        {
            consoleInterface?.WriteError(error);
        }
    }
}