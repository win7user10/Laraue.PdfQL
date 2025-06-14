using Laraue.PdfQL.Interpreter.DelegateCompiling;

namespace Laraue.PdfQL.PdfObjects;

public static class Extensions
{
    /// <summary>
    /// Get element from collection by it human-readable number. E.g. cell #1 means element with index #0.
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="cellNumber"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="PdfqlRuntimeException"></exception>
    public static T? GetByCellNumber<T>(this IReadOnlyList<T> collection, int cellNumber)
    {
        var index = cellNumber - 1;
        if (index < 0)
        {
            throw new PdfqlRuntimeException("Cell number cannot be less than 1");
        }

        return collection.Skip(index).FirstOrDefault();
    }
}