using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace Rnd.Core.ConsoleApp.DataGenerator
{
    abstract class TreeGenerator<T>
    {
        readonly Random random = new Random();

        protected abstract T CreateRoot();
        protected abstract T Create(T rootElement, [MaybeNull]T parent);

        public IEnumerable<T> GenerateWithInverseExponentialBinarySequence(int horizontalCount, int verticalCount, int totalCount)
        {
            var verticalDistribution = Enumerable.Range(0, verticalCount)
                .Select(s => Math.Pow(2, s * -1))
                .ToArray();

            return Generate(horizontalCount, verticalCount, totalCount, verticalDistribution);
        }

        public IEnumerable<T> Generate(int horizontalCount, int verticalCount, int totalCount)
        {
            var verticalDistribution = Enumerable.Range(0, verticalCount).Select(s => 1d).ToArray();
            return Generate(horizontalCount, totalCount, totalCount, verticalDistribution);
        }

        public IEnumerable<T> Generate(
            int horizontalCount, 
            int verticalCount, 
            int totalCount,
            double[] verticalDistribution,
            double[]? horizontalDistribution = null, 
            bool addRandomness = true)
        {
            var calculatedHorizontalDist = GetDistribution(horizontalCount, horizontalDistribution, addRandomness);

            for (var i = 0; i < horizontalCount; i++)
            {
                var root = CreateRoot();

                var verticalCountPerColumn = Math.Round(totalCount * calculatedHorizontalDist[i]);
                var calculatedVerticalDistribution =
                    GetDistribution(verticalCount, verticalDistribution, addRandomness);

                var elementsInLevel = new List<T>();

                for (var j = 0; j < verticalCount; j++)
                {
                    var countInLevel = Math.Round(verticalCountPerColumn * calculatedVerticalDistribution[j]);

                    if (Math.Abs(countInLevel) < double.Epsilon)
                        break;

                    var elementsInPreviousLevel = elementsInLevel;
                    elementsInLevel = new List<T>();

                    for (var k = 0; k < countInLevel; k++)
                    {
                        var parent = elementsInPreviousLevel.Any()
                            ? elementsInPreviousLevel[random.Next(0, elementsInPreviousLevel.Count - 1)]
                            : default;

                        var entity = Create(root, parent);
                        elementsInLevel.Add(entity);
                        yield return entity;
                    }
                }
            }
        }

        IReadOnlyList<double> GetDistribution(int count, IReadOnlyList<double>? distribution, bool addRandomness)
        {
            var calculatedDistribution = Enumerable.Range(0, count)
                .Select(s => addRandomness 
                    ? random.NextDouble() * distribution?[s] ?? 1 
                    : distribution?[s] ?? 1)
                .ToList();

            var normalisationParameter = calculatedDistribution.Sum();
            return calculatedDistribution.Select(s => s / normalisationParameter).ToArray();
        }
    }
}
