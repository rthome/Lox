#!/usr/bin/env python3

"""
Tool to generate AST classes for lox.
"""

import sys
import os
import io


class IndentingStringIO(io.StringIO):
    def __init__(self):
        super().__init__()
        self.indentation_level = 0

    def _indent_string(self, s):
        return ("\t" * self.indentation_level) + s

    def indent(self):
        self.indentation_level += 1

    def deindent(self):
        self.indentation_level = max(0, self.indentation_level - 1)

    def write(self, s):
        indented_string = self._indent_string(s)
        if not indented_string.endswith("\n"):
            indented_string += "\n"
        super().write(indented_string)

def define_type(writer, base_name, type_name, fields):
    writer.write("")
    writer.write("public class " + type_name + " : " + base_name)
    writer.write("{")
    writer.indent()
    for field in fields.split(", "):
        field_type, field_name = field.split(" ")
        property_name = field_name[0].upper() + field_name[1:]
        writer.write("public " + field_type + " " + property_name + " { get; set; }")
        writer.write("")
    writer.write("public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit" + type_name + base_name + "(this);")
    writer.write("")
    writer.write("public " + type_name + "(" + fields + ")")
    writer.write("{")
    writer.indent()
    for field in fields.split(", "):
        field_name = field.split(" ")[1]
        property_name = field_name[0].upper() + field_name[1:]
        writer.write(property_name + " = " + field_name + ";")
    writer.deindent()
    writer.write("}")
    writer.deindent()
    writer.write("}")

def define_visitor(writer, base_name, type_definitions):
    writer.write("public interface IVisitor<T>")
    writer.write("{")
    writer.indent()
    for type_name in [type_definition.split(":")[0].strip() for type_definition in type_definitions]:
        writer.write("T Visit" + type_name + base_name + "(" + type_name + " " + base_name.lower() + ");")
    writer.deindent()
    writer.write("}")

def define_ast(output_dir, base_name, type_definitions):
    path = os.path.join(output_dir, base_name) + ".cs"
    with IndentingStringIO() as writer:
        writer.write("namespace lox")
        writer.write("{")
        writer.indent()
        writer.write("abstract class " + base_name)
        writer.write("{")
        writer.indent()
        writer.write("public abstract T Accept<T>(IVisitor<T> visitor);")
        writer.write("")
        define_visitor(writer, base_name, type_definitions)
        for type_definition in type_definitions:
            type_name = type_definition.split(":")[0].strip()
            fields = type_definition.split(":")[1].strip()
            define_type(writer, base_name, type_name, fields)
        writer.deindent()
        writer.write("}")
        writer.deindent()
        writer.write("}")
        with open(path, "w") as file_writer:
            file_writer.write(writer.getvalue())

def  main():
    if len(sys.argv) != 2:
        print("Usage: GenerateAst.py <output directory>")
        sys.exit(1)
    output_dir = sys.argv[1]
    define_ast(output_dir, "Expr", [
        "Ternary  : Expr cond, Expr left, Expr right",
        "Binary   : Expr left, Token op, Expr right",
        "Grouping : Expr expression",
        "Literal  : object value",
        "Unary    : Token op, Expr right",
        "Variable : Token name",
    ])
    define_ast(output_dir, "Stmt", [
        "Expression : Expr expr",
        "Print      : Expr expr",
        "Var        : Token name, Expr initializer",
    ])

if __name__ == "__main__":
    main()
