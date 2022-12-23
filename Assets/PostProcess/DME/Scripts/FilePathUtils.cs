using System.Linq;

namespace DME {
    public static class FilePathUtils {
        
        /// <summary>
        /// Step one step back in folder hierarchy.
        /// </summary>
        public static string StepBack(string filePath) {
            while (filePath.Length > 0) {
                filePath = filePath.Remove(filePath.Length - 1);
                if (filePath[filePath.Length - 1] == '/')
                    return filePath;
            }
            return filePath;
        }
    }
}
