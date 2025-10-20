import dash
from dash import html, dcc
import os
import time
import networkx as nx
from networkx.drawing.nx_agraph import read_dot
import plotly.graph_objs as go
from dash.dependencies import Input, Output

DOT_FILE = "graph.dot"
last_mtime = 0  # store last modification time


def dot_to_plotly(dot_path):
    G = read_dot(dot_path)

    pos = nx.spring_layout(G)  # Can also use Graphviz positions if needed
    edge_trace = []
    node_x = []
    node_y = []
    node_text = []

    for node in G.nodes():
        x, y = pos[node]
        node_x.append(x)
        node_y.append(y)
        node_text.append(str(node))

    for edge in G.edges():
        x0, y0 = pos[edge[0]]
        x1, y1 = pos[edge[1]]
        edge_trace.append(
            go.Scatter(
                x=[x0, x1, None],
                y=[y0, y1, None],
                mode="lines",
                line=dict(width=1, color="#888"),
                hoverinfo="none",
            )
        )

    node_trace = go.Scatter(
        x=node_x,
        y=node_y,
        mode="markers+text",
        text=node_text,
        hoverinfo="text",
        marker=dict(showscale=False, color="skyblue", size=20, line_width=2),
    )

    fig = go.Figure(
        data=edge_trace + [node_trace],
        layout=go.Layout(
            title="Interactive Graph from .dot File",
            titlefont_size=16,
            showlegend=False,
            hovermode="closest",
            margin=dict(b=20, l=5, r=5, t=40),
            xaxis=dict(showgrid=False, zeroline=False),
            yaxis=dict(showgrid=False, zeroline=False),
        ),
    )
    return fig


# Initial graph
initial_fig = dot_to_plotly(DOT_FILE)
last_mtime = os.path.getmtime(DOT_FILE)

# Dash app
app = dash.Dash(__name__)
app.title = "Live DOT Viewer (Interactive)"

app.layout = html.Div(
    [
        html.H2("📈 Interactive .dot Graph Viewer (Auto-Updating)"),
        dcc.Graph(id="graph", figure=initial_fig),
        dcc.Interval(id="interval", interval=1000, n_intervals=0),  # 1 second polling
    ]
)


@app.callback(Output("graph", "figure"), Input("interval", "n_intervals"))
def update_graph(n):
    global last_mtime
    try:
        current_mtime = os.path.getmtime(DOT_FILE)
        if current_mtime != last_mtime:
            print("Change detected. Reloading graph...")
            last_mtime = current_mtime
            return dot_to_plotly(DOT_FILE)
    except Exception as e:
        print(f"Error reading .dot file: {e}")
    # Return current figure to avoid flickering
    return dash.no_update


if __name__ == "__main__":
    app.run_server(debug=True)
