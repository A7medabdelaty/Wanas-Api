using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wanas.Application.Interfaces
{
    public interface IEmbeddingService
    {
        Task<float[]> GenerateEmbeddingsAsync(string text);
        Task<List<float[]>> GenerateEmbeddingsAsync(List<string> texts);
    }
}
