import marimo

__generated_with = "0.16.5"
app = marimo.App()


@app.cell(hide_code=True)
def _():
    import marimo as mo
    return (mo,)


@app.cell(hide_code=True)
def _(mo):
    _df = mo.sql(
        f"""
        SELECT "Channel name" AS Channel, count(*) FROM READ_CSV('Music.csv', DELIM = '~') GROUP BY "Channel name" ORDER BY count(*) DESC
        """
    )
    return


@app.cell(hide_code=True)
def _(mo):
    _df = mo.sql(
        f"""
        SELECT
            *
        FROM
            READ_CSV('Music.csv', DELIM = '~')
        """
    )
    return


if __name__ == "__main__":
    app.run()
