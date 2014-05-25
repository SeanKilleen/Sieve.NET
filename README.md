Sieve.NET
=========

An expression-based filter library for .NET. We aim for a fluent interface to build expressions to filter complex business objects.

I am new to both expressions and open source, so this will be as much an experiment as it is an open-source repo.

![Image](http://www.markderksen.ca/wp-content/uploads/2013/06/tumblr_lglb2dJGeL1qzoxl6o1_500.jpg)

(Credit: http://www.markderksen.ca/)

**This is pre-alpha. There's literally nothing here yet.**

What Problems are we Trying to Solve?
---
Problem 1: Creating filters that can be expressions
---
Okay, so here's the issue:

* We need something that will filter objects
* We need to be able to compile it, or use it as an expression (e.g. to feed it to an ORM so it can become a SQL query).
* We often need to construct and apply a lot of filters at once

So instead of creation an expression manually, or having to pick stuff out of a query string, we thought it would be nice to do something like:

    new string valuesThatComeFromSomewhere = "killeen, smith, harris";

    // only 
    new Sieve<Person, EqualitySieve>("LastName")
        .ForValues(valuesThatComeFromSomewhere)
        .UseSeparator(",")
        .RemoveWhitespace()
        .EmptyValuesBehavior(EmpyValuesBehavior.AllowAll);

Problem 2 -- Filters as Findable Classes
---
Say you have a search box that has 10 criteria and it passes those in via a query string, such as:

> /api/MySearch?LastName=Washington|Lincoln|Hamilton&Position=President&Location=DC|MD|VA

It's super annoying to do the following (pseudo-code):

    // If (FilterExists("LastName"))
       // build a LastName filter
    // If (FilterExists("Location"))
       // etc. etc.

Instead, wouldn't it be cool to do something like:

	[Sieve("MyUniqueFilterName", "LastName")]
    public class PersonLastNameFilter : ISieve<Person>
    { 
		return new Sieve<Person, EqualitySieve>("LastName");
    } 

And then find all the filters via:

	new SieveLocator<Person>().GetFiltersForPropertyName("LastName");

Or, take the QueryString / NameValueCollection Itself and Parse it:

     var nvc = ConvertQueryStringToNameValueCollection(queryString);
     var filters = newSieveLocator().GetSieves(nvc); // instances of all filters, ready to go.

Roadmap / Goals
===
* Ability to Create an Equality Sieve for one value on a given object's `int` property
 * Then, `string` properties
 * Then, `long` properties
 * Then, `Date` properties
* Ability to create Sieve for Multiple Values
* Ability to "and" the inner values together instead of "or" ing them.
* Ability to change the separator
* Ability to dictate what happens with an invalid value (ignore it? throw exception?)
* Ability to dictate what happens with an empty list of values (let all through? none through? Throw exception?)
* Ability to find Sieves for an object
* Search: Ability to retrieve Sieves based on object and property name
* Search: Assume the property name if it's not given
* Search: Ability to retrieve Sieves with values from a name value collection
* Search: Ability to retrieve sieves without values from a name value collection (to do more work on them)
* Ability to combine Sives into one large Sieve (manually or via an extension method on `IEnumerable<Sieve>`
* Other Sieve Types
 * `GreaterThan` and `LessThan` Sieves (with inclusive and exclusive options)
 * `Contains` Sieve
 * Case-insensitivity options, etc.