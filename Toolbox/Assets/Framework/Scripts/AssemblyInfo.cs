using System.Runtime.CompilerServices;

// The registry tool lives in the editor assembly and writes the generated manifest and scene paths.
// Those entry points stay non-public so an example author never calls them by accident.
[assembly: InternalsVisibleTo("Assembly-CSharp-Editor")]
