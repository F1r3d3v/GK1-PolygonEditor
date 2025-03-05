# GK1-PolygonEditor

## Project Description

GK1-PolygonEditor is a Windows Forms application designed for creating and manipulating 2D polygon shapes. It offers a user-friendly interface to draw polygons, apply geometric constraints (horizontal, vertical, fixed length), create Bezier curves, and manage vertex continuity (G0, G1, C1). The editor supports two rendering modes: the standard .NET library rendering and a custom Bresenham's line algorithm implementation with Xiaolin Wu's antialiasing.

## Table of Contents

1.  [Project Description](#project-description)
2.  [Installation Instructions](#installation-instructions)
3.  [Usage Guide](#usage-guide)
    *   [Creating Shapes](#creating-shapes)
    *   [Moving Vertices and Shapes](#moving-vertices-and-shapes)
    *   [Applying Constraints](#applying-constraints)
    *   [Bezier Curves](#bezier-curves)
    *   [Renderer Selection](#renderer-selection)
    *   [Camera Control](#camera-control)
    *   [Context Menus](#context-menus)
4.  [Constraint Mechanism](#constraint-mechanism)
    *   [Core Concepts](#core-concepts)
    *   [Types of Constraints](#types-of-constraints)
    *   [How Constraints are Enforced?](#how-constraints-are-enforced-algorithm)
    *   [Limitations](#limitations)

## Installation Instructions

1.  **Prerequisites:**
    *   .NET 8.0 SDK (with Windows Desktop Runtime and WinForms)

2.  **Clone the Repository:**

    ```bash
    git clone https://github.com/F1r3d3v/GK1-PolygonEditor.git
    cd GK1-PolygonEditor
    ```

3.  **Build the Project:**

    *   **Using Visual Studio:**
        1.  Open `GK1-PolygonEditor.sln` in Visual Studio.
        2.  Build the solution (Build -> Build Solution or Ctrl+Shift+B).

    *   **Using the .NET CLI:**
        1.  Navigate to the project directory in the terminal.
        2.  Run `dotnet build`.

4.  **Run the Application:**

    *   **Using Visual Studio:**
        1.  Press F5 or click the "Start" button.

    *   **Using the .NET CLI:**
        1.  Navigate to the `bin/Debug/net8.0-windows` (or `bin/Release/net8.0-windows` if you built in Release mode) directory.
        2.  Run `dotnet GK1-PolygonEditor.dll`.  (You may also be able to simply double-click the `GK1-PolygonEditor.exe` file.)

## Usage Guide

### Creating Shapes

*   **Start a New Shape:** Double-click on the canvas to begin creating a new polygon.
*   **Add Vertices:**  Left-click on the canvas to add vertices to the current shape.
*   **Close Shape:**  To close a shape, left-click on the first vertex of the shape.  A shape must have at least three vertices to be closed.  If you are creating a shape and want to abandon it, right-click.
* **Split Edge**: Right-click on an existing edge and select `Split edge`.

### Moving Vertices and Shapes

*   **Move a Vertex:** Left-click and drag a vertex to reposition it.  The application will attempt to maintain any constraints applied to adjacent edges.
*   **Move a Shape:** Left-click and drag inside a closed shape to move the entire shape.
*   **Move control point:** Bezier curves have control points, which can be dragged like any other vertex.

### Applying Constraints

*   **Edge Constraints:** Right-click on an edge to access the context menu:
    *   **Horizontal:** Constrains the edge to be horizontal.
    *   **Vertical:** Constrains the edge to be vertical.
    *   **Fixed Length:**  Prompts you to enter a desired length for the edge.  The application will attempt to maintain this length when you move connected vertices.
    *   **Remove Constraint:** Removes any existing constraint from the edge.
*   **Vertex Continuity Constraints:** Right-click a vertex where two Bezier curves meet:
    *  **G0:** Provides simple positional continuity
    *   **G1:** Ensures that the tangents of the two curves at the vertex are aligned (same direction, but potentially different magnitudes).
    *   **C1:**  Ensures that the tangents are aligned and have the same magnitude (first derivatives are equal).

### Bezier Curves

*  **Change to Bezier:** Right-click on existing edge and select `Change to Bezier`. This will create a Bezier curve and display the Bezier control points.
*   **Change to Edge:** Right-click on Bezier curve and select `Change to Edge` to convert the curve back to a straight edge.

### Renderer Selection

*   **Renderer Options:** Use the radio buttons in the "Renderer" group box on the right side of the application to choose between:
    *   **Library:**  Uses the built-in .NET graphics library for drawing.
    *   **Bresenham:** Uses a custom implementation of Bresenham's line algorithm, with Wu's antialiasing as an option.

### Camera Control

*   **Pan:** Right-click and drag on an empty area of the canvas to pan the camera view.  The coordinates displayed in the status bar will update accordingly.

### Context Menus

*   **Vertex Context Menu:** Right-click on a vertex to:
    *   **Delete vertex:** Deletes the vertex and attempts to reconnect the adjacent edges.  If the shape has only three vertices, deleting a vertex will delete the entire shape.
    *   **Set continuity:** Change continuity constraints.
*   **Edge Context Menu:** Right-click on an edge to:
    *   **Split edge:** Adds a new vertex in the middle of the edge.
    * **Change to bezier:** Creates a Bezier curve.
    *   **Set constraint:** Apply horizontal, vertical, or fixed length constraints.
    *   **Remove constraint:** Removes an existing constraint.
    * **Toggle antialiasing:** Turns antialiasing On/Off (only if Bresenham renderer is active).
* **Bezier curve Context Menu:** Right-click on bezier curve to:
    * **Change to edge:** Changes bezier curve to edge.
    * **Toggle antialiasing:** Turns antialiasing On/Off (only if Bresenham renderer is active).
*   **Shape Context Menu:** Right-click on a shape to delete it.

## Constraint Mechanism

The GK1-PolygonEditor provides a robust constraint system that allows users to define and maintain geometric relationships between parts of a polygon.  This system ensures that when you modify one part of a shape (e.g., move a vertex), other parts adjust automatically to preserve the specified constraints.

### Core Concepts

*   **Constraints as Relationships:** Constraints define relationships between geometric elements (vertices and edges).  For example, a "horizontal" constraint on an edge establishes a relationship that forces its two endpoints to always have the same Y-coordinate.

*   **Constraint Enforcement:** The editor actively enforces constraints whenever a user interacts with the shape.  This is primarily done in the `Shape.PreserveConstraints()` method, which is called after a vertex is moved.

*   **Propagation:** Constraint enforcement often requires *propagation*.  Moving one vertex to satisfy a constraint on one edge might violate a constraint on an adjacent edge.  The editor handles this by iteratively adjusting connected vertices until all constraints are satisfied. If no resolution is possible, the shape is moved.

* **Prioritization:** The editor uses the order that segments are connected, in clockwise and anti-clockwise directions from vertex that was moved, to resolve conflicts.

### Types of Constraints

The editor supports the following constraint types:

1.  **Edge Constraints:**
    *   **Horizontal:** Forces an edge to remain horizontal.  This is implemented by ensuring the Y-coordinates of the edge's start and end vertices are always equal.
    *   **Vertical:** Forces an edge to remain vertical.  This is implemented by ensuring the X-coordinates of the edge's start and end vertices are always equal.
    *   **Fixed Length:** Maintains a specified distance between the edge's start and end vertices. When other vertices are moved, the editor adjusts the position of *one* of the edge's endpoints to keep the length constant.

2.  **Vertex Continuity Constraints (for Bezier Curves):**
    *   **G0 Continuity:** This is the basic connection between two Bezier curve segments. It simply ensures that the end point of the first curve is the same as the start point of the second curve. There's no constraint on the tangents.
    *   **G1 Continuity:** This constraint ensures that the tangent vectors at the joining point of two Bezier curves are *collinear* (point in the same or opposite direction).  The *magnitudes* of the tangent vectors can be different.  This creates a smooth visual transition, but the "speed" of the curve can change abruptly.  The `PreserveContinuity()` method (and related `ForceContinuityVisitor`) handles G1 by adjusting control points to lie on the same line.
    *   **C1 Continuity:** This is a stricter form of G1 continuity.  C1 continuity requires that the tangent vectors at the joining point are not only collinear but also have the *same magnitude*.  This results in a smoother transition, with no abrupt change in the "speed" of the curve.  `PreserveContinuity()` achieves C1 continuity by moving control points.

### How Constraints are Enforced? (Algorithm)

The constraint enforcement algorithm is triggered whenever a vertex is moved.  Here's a simplified breakdown:

1.  **Initial Move:** The user moves a vertex (the "moved vertex").

2.  **Clockwise and Counterclockwise Traversal:** The `Shape.PreserveConstraints()` method performs two traversals of the shape's segments, starting from the moved vertex: one clockwise and one counterclockwise.

3.  **Constraint Checking:** For each segment encountered during the traversal:
    *   Check if the segment has a constraint.
    *   If a constraint exists, check if it's currently *preserved* (using the `IsPreserved()` method of the constraint).
    *   If the constraint is *not* preserved, attempt to adjust the position of the *other* vertex of the segment (not the moved vertex) to satisfy the constraint. For example:
        *   **Horizontal/Vertical:** Adjust the X or Y coordinate of the other vertex.
        *   **Fixed Length:** Recalculate the position of other vertex, to preserve length, keeping in mind last move.

4.  **Continuity Preservation (Bezier Curves):** After the constraint checks, `Shape.PreserveContinuity()` is called. This method adjusts the control points of Bezier curves to maintain G0, G1, or C1 continuity at the moved vertex, based on any continuity constraints that are set.

5. **Whole polygon move:** If during clockwise check, algorithm find that the constraints cannot be satisfied, the polygon is moved.

### Limitations

*   **Over-constrained Systems:**  If a shape has too many conflicting constraints, the editor might not be able to perfectly satisfy all of them.
*   **Two Adjacent constraints of same type:** For example two horizontal edges connected to same vertex, are not allowed.
