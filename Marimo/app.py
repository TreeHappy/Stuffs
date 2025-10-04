import marimo

__generated_with = "0.16.5"
app = marimo.App()


@app.cell
def _():
    import marimo as mo
    import duckdb as dbd
    import sqlglot as ss

    return (mo,)


@app.cell
def _(mo):
    _df = mo.sql(
        f"""
        SELECT * FROM READ_CSV('Music.csv')
        """
    )
    return


@app.cell
def _():
    import duckdb

    DATABASE_URL = "Music.csv"
    engine = duckdb.connect(DATABASE_URL, read_only=True)
    return


if __name__ == "__main__":
    app.run()
