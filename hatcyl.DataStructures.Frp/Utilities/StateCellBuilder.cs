using Sodium.Frp;

namespace hatcyl.DataStructures.Frp.Utilities;
public record class StateCellBuilder<TState>(TState InitialState)
{
    private TState InitialState { get; init; } = InitialState;
    private IEnumerable<Stream<Func<TState, TState>>> Methods { get; init; } = Enumerable.Empty<Stream<Func<TState, TState>>>();

    public StateCellBuilder<TState> WithMethod<T>(Stream<T> input, Func<T, Func<TState, TState>> logic) =>
        this with { Methods = Methods.Append(input.Map(logic)) };

    public StateCellBuilder<TState> WithMethod<T>(Func<T, Func<TState, TState>> logic, params Stream<T>[] inputs) =>
        this with { Methods = Methods.Append(OrElse(inputs).Map(logic)) };

    public Cell<TState> Build() =>
        Loop(loop => Merge(Methods).Snapshot(loop, (f, state) => f(state)).Hold(InitialState));

    private Stream<T> OrElse<T>(IEnumerable<Stream<T>> streams) =>
        streams.Count() == 1 ? streams.Single() : OrElse(streams.Skip(2).Prepend(streams.First().OrElse(streams.Skip(1).First())));

    public static Stream<Func<TState, TState>> Merge(IEnumerable<Stream<Func<TState, TState>>> streams) =>
        streams.Count() == 1 ? streams.Single() : Merge(streams.Skip(2).Prepend(streams.First().Merge(streams.Skip(1).First(), (x, y) => s => x(y(s)))));

    private Cell<TState> Loop(Func<Cell<TState>, Cell<TState>> f) =>
        Transaction.Run
        (() =>
        {
            CellLoop<TState> cellLoop = Cell.CreateLoop<TState>();
            cellLoop.Loop(f(cellLoop));
            return cellLoop;
        }
        );
}