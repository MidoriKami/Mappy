using FFXIVClientStructs.STD;

namespace Mappy.Extensions;

public static class StdListExtensions {
    public unsafe ref struct StdListEnumerator<T>(StdList<T> list)
        where T : unmanaged {
        private ulong currentIndex = 0;
        private StdList<T>.Node* current = list.Head;

        public bool MoveNext() {
            if (currentIndex < list.Size) {
                if (current is not null && current->Next is not null) {
                    current = current->Next;
                    currentIndex++;
                    return true;
                }
            }

            return false;
        }

        public readonly ref T Current => ref current->Value;
        public StdListEnumerator<T> GetEnumerator() => new(list);
    }

    public static StdListEnumerator<T> GetEnumerator<T>(this StdList<T> list) where T : unmanaged => new(list);
}