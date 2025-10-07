import marimo

__generated_with = "0.16.5"
app = marimo.App()


@app.cell
def _():
    import marimo as mo
    return (mo,)


@app.cell
def _(mo):
    _df = mo.sql(
        f"""
        SELECT * FROM READ_CSV('Music.csv', DELIM='~')
        """
    )
    return


if __name__ == "__main__":
    app.run()
