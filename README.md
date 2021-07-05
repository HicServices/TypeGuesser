# Type Guesser

[![Build status](https://github.com/HicServices/TypeGuesser/workflows/Build/badge.svg)](https://github.com/HicServices/TypeGuesser/actions?query=workflow%3ABuild) [![Total alerts](https://img.shields.io/lgtm/alerts/g/HicServices/TypeGuesser.svg?logo=lgtm&logoWidth=18)](https://lgtm.com/projects/g/HicServices/TypeGuesser/alerts/)  [![NuGet Badge](https://buildstats.info/nuget/HIC.TypeGuesser)](https://buildstats.info/nuget/HIC.TypeGuesser)

Guess the C# Types for untyped strings e.g. `"12.123"`.

- [Nuget Package](https://www.nuget.org/packages/HIC.TypeGuesser/)
- [License MIT](./LICENSE)

Usage

```csharp
var guesser = new Guesser();
guesser.AdjustToCompensateForValue("-12.211");
var guess = guesser.Guess;
```

The resulting guess in this case would be:

|Property  | Value |
|-------|----|
|   `guess.CSharpType`| `typeof(decimal)` |
|   `guess.Size.NumbersBeforeDecimalPlace` | 2 |
|   `guess.Size.NumbersAfterDecimalPlace`| 3 |
|   `guess.Width` | 7 |


Guesser also handles adjusting it's guess based on multiple input strings e.g.


```csharp
var guesser = new Guesser();
guesser.AdjustToCompensateForValue("1,000");
guesser.AdjustToCompensateForValue("0.001");
var guess = guesser.Guess;
```

|Property  | Value |
|-------|----|
|   `guess.CSharpType`| `typeof(decimal)` |
|   `guess.Size.NumbersBeforeDecimalPlace` | 4 |
|   `guess.Size.NumbersAfterDecimalPlace`| 3 |
|   `guess.Width` | 8 |

Once you have guessed a Type for all your strings you can convert all your values to the hard type:

```csharp
var someStrings = new []{"13:11:59", "9AM"};
var guesser = new Guesser();
guesser.AdjustToCompensateForValues(someStrings);

var parsed = someStrings.Select(guesser.Parse).ToArray();

Assert.AreEqual(new TimeSpan(13, 11, 59), parsed[0]);
Assert.AreEqual(new TimeSpan(9, 0, 0), parsed[1]);
```

String parsing/converting is largely handled by the excellent [Universal Type Converter](https://github.com/t-bruning/UniversalTypeConverter) project by [t-bruning](https://github.com/t-bruning).

# Guess Order
The order in which Types are tried is (`DatabaseTypeRequest.PreferenceOrder`):

- Bool
- Int
- Decimal
- TimeSpan
- DateTime 
- String

If a string has been accepted as one category e.g. "12" (`Int`) and an incompatible string arrived e.g. "0.1" then the `Guess` is changes to either the new Type (`Decimal`) or to String (i.e. untyped) based on whether the old and new Types are in the same `TypeCompatibilityGroup`

For example `Bool` and `DateTime` are incompatible

```
"Y" => Bool
"2001-01-01" => DateTime

Guess: String
```

Guesses are never revised back up again (once you accept a `Decimal` you never get `Int` again but you might end up at `String`)

### Zero Prefixes
If an input string is a number that starts with zero e.g. "01" then the estimate will be changed to `System.String`.  This is intended behaviour since some codes e.g. CHI / Barcodes have valid zero prefixes.  If this is to be accurately preserved in the database then it must be stored as string (See `TestGuesser_PreeceedingZeroes`).  This also applies to values such as "-01"

### Whitespace
Leading and trailing whitespace is ignored for the purposes of determining Type.  E.g. " 0.1" is a valid `System.Decimal`.  However it is recorded for the maximum Length required if we later fallback to `System.String` (See Test `TestGuesser_Whitespace`).

### Strong Typed Objects

Guesser.AdjustToCompensateForValue takes a `System.Object`.  If you are passing objects that are not `System.String` e.g. from a `DataColumn` that has an actual Type on it (e.g. `System.Float`) then `Guesser` will set the `Guess.CSharpType` to the provided object Type.  It will still calculate the `Guess.Size` properties if appropriate (See test `TestGuesser_HardTypeFloats`).

The first time you pass a typed object (excluding DBNull.Value) then it will assume the entire input stream is strongly typed (See `IsPrimedWithBonafideType`).  Any attempts to pass in different object Types in future (or if strings were previously passed in before) will result in a `MixedTypingException`.
