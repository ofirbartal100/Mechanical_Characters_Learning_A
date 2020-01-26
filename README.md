# Mechanical Characters Learning A

This project is a side tool to the ***[Mechanical Characters](https://github.com/ofirbartal100/Mechanical_Characters)*** project

Here you can train the weigths of A with an iterative interface

## Usage
For using this app you need to compile with VS2017.
Also when running for the first time an error should occure, saying you need to define the paths for your Python, and Script files.

### Config File
For the app to run, a config file needs to be set.
1) Make sure you have the setup for [Mechanical Characters](https://github.com/ofirbartal100/Mechanical_Characters).
2) Run the app.
3) Press a button and get the error message.
4) Close the app, and go to file location (specified in the error message).
5) Update the paths as needed.
6) Run the app again.

*The script is in the other git project "[Mechanical Characters](https://github.com/ofirbartal100/Mechanical_Characters)" => "generate_random_curve_for_learning_A.py"

Example of configured file:
```
[
  {
    "Type": "System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c123434e089",
    "Name": "PythonPath",
    "Value": "C:\\Users\\a\\Anaconda3\\envs\\mc\\python.exe"
  },
  {
    "Type": "System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c123434e089",
    "Name": "PythonScriptPath",
    "Value": "C:\\Users\\a\\mechanical_characters\\generate_random_curve_for_learning_A.py"
  }
]
```

# Related Projects

For [Mechanical Characters](https://github.com/ofirbartal100/Mechanical_Characters)

For [Mechanical Characters UI](https://github.com/ofirbartal100/MechanicalCharactersUI)
