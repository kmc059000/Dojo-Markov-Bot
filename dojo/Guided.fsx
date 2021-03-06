﻿(*
All source material for this dojo can be found at:
https://github.com/c4fsharp/Dojo-Markov-Bot

It's MIT licensed - use it, share it, modify it.
*)

(*
Introduction
+++++++++++++++++++++++++++++++++++++++++++++++++

The goal of the dojo is to create a program that
generates human-looking sentences, by reproducing
patterns that exist in a text sample.

To achieve this, we will break the sample into
n-grams. A n-gram is a succession of n words in
the text. n-grams capture common word sequences;
If we generate text following existing n-grams,
we'll get a "plausible" sequence of words, 
without having to understand how language works.

We'll guide you through writing a markov bot
generating text based on bigrams (sequences of 2
words), and then suggest directions to 
explore further!
*)

open System.Text.RegularExpressions

// Sample text: "What a Wonderful World"
// http://en.wikipedia.org/wiki/What_a_Wonderful_World

let sample = """
I see trees of green, red roses, too,
I see them bloom, for me and you
And I think to myself
What a wonderful world.

I see skies of blue, and clouds of white,
The bright blessed day, the dark sacred night
And I think to myself
What a wonderful world.

The colors of the rainbow, so pretty in the sky,
Are also on the faces of people going by.
I see friends shaking hands, sayin', "How do you do?"
They're really sayin', "I love you."

I hear babies cryin'. I watch them grow.
They'll learn much more than I'll ever know
And I think to myself
What a wonderful world

Yes, I think to myself
What a wonderful world"""

(* 
Chapter 1: Breaking the input into bi-grams
+++++++++++++++++++++++++++++++++++++++++++++++++

Your goal here is to take the input text, and
break it into a collection of bigrams, that is, 
consecutive words. For instance, 
"the colors of the rainbow" produces 4 bigrams:
- the,colors
- colors,of
- of,the
- the,rainbow
*)


// TODO: WRITE A FUNCTION 
// let bigramify (text:string) = ...
// THAT BREAKS A TEXT INTO AN ARRAY OF BIGRAMS



let regexReplace (regex:Regex) (text:string) =
    regex.Replace(text, " ")

let cleanseText (text:string) =
    let regex = new Regex("[\s.,?]+")
    let replace = regexReplace regex
    let fullReplace = replace >> replace
    text |> fullReplace

let split (text:string) =
    text.Split(' ')

let ngramify n (text:string) =
    text
    |> cleanseText
    |> split
    |> Seq.windowed n
    |> Seq.toArray

let bigramify = ngramify 2
let trigramify = ngramify 3

let bigrams = bigramify sample
let trigrams = trigramify sample

(* 
Chapter 2: Finding next word candidates
+++++++++++++++++++++++++++++++++++++++++++++++++

Now that you have isolated bi-grams, given a 
starting word, you will need to find all the 
words that could follow it. To do this, you need
to extract all the bi-grams where the first word
matches, and return all the next words in the 
selected bi-grams, in an array.
*)

// TODO: WRITE A FUNCTION 
// let nextWords ... = ...
// THAT RETURNS ALL WORDS FOLLOWING A GIVEN WORD.

let nextWords (bigrams:string[] seq) (word:string) =
    bigrams
    |> Seq.filter (fun b -> b.[0] = word)
    |> Seq.map (fun b -> b.[1])
    |> Seq.distinct
    |> Seq.toArray

nextWords bigrams "I"

//determines if the ngram matches the word sequence. returns true if the ngram matches the passed in words
let isNgramMatch (ngram:string[]) (words:string[]) =
    ngram
    |> Seq.zip words
    |> Seq.fold (fun acc elem -> 
        let a, b = elem
        acc && a = b)
        true
    
let nNextWords (ngrams:string[] seq) (words:string[]) =
    let lastWord (ngram:string[]) = ngram.[ngram.Length - 1]
    ngrams
    |> Seq.filter (fun b -> isNgramMatch b words)
    |> Seq.map lastWord
    |> Seq.distinct
    |> Seq.toArray

nextWords bigrams "I"
nNextWords bigrams [| "I" |]
nNextWords trigrams [| "I"; "see" |]
    
(* 
Chapter 3: Generating a "sentence"
+++++++++++++++++++++++++++++++++++++++++++++++++

Almost there! The only thing we need now is to
start from an initial word, find a next word 
using the bi-grams from the sample, append it to
the sentence, and repeat until we find no next
word. There are many ways to do this, recursion
being one option.
*)


// TODO: WRITE A FUNCTION 
// let generateWords ... = ...
// THAT BREAKS SAMPLE TEXT INTO BI-GRAMS, AND
// STARTING FROM AN INPUT WORD, PRODUCES A
// SENTENCE BY APPENDING WORDS.

let generateWords ngramSize (sample:string) (firstWords:string[]) =
    let rand = new System.Random()
    let ngrams = ngramify ngramSize sample
    let nextWords = nNextWords ngrams

    let rec addWordToSentence sentence lastWords count =
        let nextWordOptions = nextWords lastWords
        if nextWordOptions.Length = 0 || count = 0
            then sentence
            else
                let idx = rand.Next() % nextWordOptions.Length
                let nextWord = nextWordOptions.[idx]
                let newSentence = sentence + " " + nextWord
                let lastWords = Array.append (lastWords |> Array.skip 1) [| nextWord |]
                addWordToSentence newSentence lastWords (count - 1)
    
    let sentence = firstWords |> String.concat " "
    addWordToSentence sentence firstWords 1000

let bigramGenerateWords = generateWords 2
let trigramGenerateWords = generateWords 2

generateWords 2 sample [| "hear" |]
generateWords 3 sample [| "I"; "see" |]
generateWords 4 sample [| "I"; "see"; "skies" |]


(* 
Next episode: Have fun!
+++++++++++++++++++++++++++++++++++++++++++++++++

By now, you should have a function generateWords
that produces "variations" based on the song
"What a Wonderful World".

The next step is entirely up to you!

Here are a couple of ideas you could explore:

1. The song is fairly short, and doesn't have
many sentences/patterns. You will get more 
interesting/fun results using longer samples. Or
mixing samples of different origins.

Project Gutenberg is a great resource for this;
it contains public domain classics:
https://www.gutenberg.org/
We included two novels from there, included as
raw text files (jeeves.txt and tarzan.txt):
https://www.gutenberg.org/ebooks/8164
https://www.gutenberg.org/ebooks/81

We also included a home-made compilation of
TechCrunch blog posts, initrode.txt, which can be
used to make a fun startup idea generator :)

2. Bi-grams are short patterns. You can get more
"realistic looking" results by using tri-grams, 
or 4, 5, ... - grams. For instance, with 
tri-grams, you would look for sequences of 3
words, like "I/see/trees", "I/see/them", ... and
generate forward:
- starting with "I/see/" 
- pick (for instance) "them",
- continue with "see/them"

3. Improve performance, by storing n-grams in a
suitable data structure?

4. Being smarter when parsing the sample. 
Splitting on white space is very crude, and makes
it difficult to find (for instance) where a
sentence starts or ends, and identifying good
points to stop generating words.

5. Create a Markov buddy: write a bot that 
responds to user input, with a sentence that is
"reasonable". 

So... have fun, and hopefully produce realistic
or fun "human looking text"
*)