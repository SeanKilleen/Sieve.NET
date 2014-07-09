Sieve.NET
=========
[![Build status](https://ci.appveyor.com/api/projects/status/0h8ong5gc43bops4)](https://ci.appveyor.com/project/SeanKilleen/sieve-net)

An expression-based filter library for .NET. We aim for a fluent interface to build expressions to filter complex business objects.

I am new to both expressions and open source, so this will be as much an experiment as it is an open-source repo.

![Image](http://www.markderksen.ca/wp-content/uploads/2013/06/tumblr_lglb2dJGeL1qzoxl6o1_500.jpg)

(Credit: http://www.markderksen.ca/)

**This is pre-alpha. Jump in and help us make it official!.**

Jump in!
====
* Attempting to start a chat at http://chat.stackoverflow.com/rooms/55080/sieve-net so we can discuss things on a regular basis.

What Problems are we Trying to Solve?
---
Problem 1: Creating filters that can be expressions
---
Okay, so here's the issue:

* We need something that will filter objects
* We need to be able to compile it, or use it as an expression (e.g. to feed it to an ORM so it can become a SQL query).
* We often need to construct and apply a lot of filters at once

So instead of creating an expression manually, or having to pick stuff out of a query string, we thought it would be nice to do something like:

    new string valuesThatComeFromSomewhere = "killeen, smith, harris";

    // only 
    new EqualitySieve<Person>()
		.ForProperty(x => x.LastName)
        .ForValues(valuesThatComeFromSomewhere)
        .WithSeparator(",")
        .EmptyValuesBehavior(EmpyValuesBehavior.LetAllObjectsThrough);

Problem 2 -- Filters as Findable Classes
---
Say you have a search box that has 10 criteria and it passes those in via a query string, such as:

> /api/MySearch?LastName=Washington|Lincoln|Hamilton&Position=President&Location=DC|MD|VA

It's super annoying to do the following (pseudo-code):

    // If (FilterExists("LastName"))
       // build a LastName filter
    // If (FilterExists("Location"))
       // etc. etc.

Instead, wouldn't it be great to be able to do something like:

	[Sieve("MyUniqueFilterName", "LastName")]
    public class PersonLastNameFilter : IFindableSieve<Person>
    { 
		public Sieve<Person> GetSieve()
		{
			return new EqualitySieve<Person>()
				.ForProperty(x => x.LastName);
		}
    } 

And then find all the filters via:

	new SieveLocator<Person>().GetFiltersForPropertyName("LastName");

Or, take the QueryString / NameValueCollection Itself and Parse it:

     var nvc = ConvertQueryStringToNameValueCollection(queryString);
     var filters = newSieveLocator().GetSieves(nvc); // instances of all filters, ready to go.

Roadmap / Goals
===
See the [Issues section for this repo](https://github.com/SeanKilleen/Sieve.NET/issues). I'll do my best to not let it stagnate.
