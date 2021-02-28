using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using ServerConfig;
using System;
using System.Collections.Generic;

namespace SlotMachineBackend{
    public class Symbol{
        public int value { get; set; }
        public String emoteId { get; set; }

        public float multiplier { get; set; } = 1f;
    
        public Symbol(int value) 
        {
            this.value = value;
            this.emoteId = null;
        }
        
        public override string ToString()
        {
            return $"val: {value}, multiplier: {multiplier}";
        }

        /*
        public override string ToString()
        {
            return $"value: {value}, emoteId: {emoteId}";
        }
        */
    }

    public class CircularList<T> : List<T>{
        
        public int index { get; set; } = 0;

        public T Next(){
            if(index >= this.Count){
                index = 0;
            }
            T vToReturn = this[index];
            index = index + 1;
            return vToReturn;
        }

        public void RandomizeIndex(){
            Random random = new Random();  
            this.index = random.Next(0, this.Count);  
        }
    }

     public class Slot{

        public CircularList<Symbol> carousel { get; set; }    

        public Slot(CircularList<Symbol> symbols){
            this.carousel = new CircularList<Symbol>();

            Symbol lastSymbol = null;
            foreach(var symbol in symbols){
                
                this.carousel.Add(symbol);
                
                if(lastSymbol != null){
                    this.carousel.Add(lastSymbol);

                    this.carousel.Add(symbol);

                    this.carousel.Add(lastSymbol);

                    this.carousel.Add(symbol);
                }

                lastSymbol = symbol;
            }
            
            symbols.Reverse();
            lastSymbol = null;
            
            foreach(var symbol in symbols){

                if(lastSymbol != null){

                    this.carousel.Add(symbol);

                    this.carousel.Add(lastSymbol);

                    this.carousel.Add(symbol);

                    this.carousel.Add(lastSymbol);
                }

                lastSymbol = symbol;
            }
        }
        
        public List<Symbol> getSample(){
            List<Symbol> symbolSample = new List<Symbol>();

            this.carousel.RandomizeIndex();

            for(int i = 0; i < 3; i++){
                symbolSample.Add(carousel.Next());
            }

            return symbolSample;
        }
    }



    public class SlotMachine {

        public float Balance { get; set; } = 0f;
        public CircularList<Symbol> symbols { get; set; }
        public List<string> utilityPatternList = new List<string>(){
            "STRAIGHT TOP", 
            "STRAIGHT MID", 
            "STRAIGHT BOT",
            "DIAG ONE",
            "DIAG TWO",
            "DIAMOND",
            "CORNERS",
            "STRAIGHT ASCENDING MID"};
        public Slot virtualSlot { get; set; }
        public Dictionary<string, float> patternMultipliers { get; set; } = new Dictionary<string, float>();
        public Dictionary<string, int> patternMatches { get; set; } = new Dictionary<string, int>();
        public Dictionary<Symbol, int> symbolMatches { get; set; } = new Dictionary<Symbol, int>();
        
        public SlotMachine(int numberSymbols){

            this.symbols = new CircularList<Symbol>();

            for(int i = 1; i <= numberSymbols; i++){
                this.symbols.Add(new Symbol(i));
            }

            this.virtualSlot = new Slot(this.symbols);
        }

        public void reloadSymbols(){
            this.virtualSlot = new Slot(this.symbols);
        }

        public  Dictionary<string, Symbol> Roll(){

            reloadSymbols();

            List<Symbol> leftSlot = virtualSlot.getSample();
            List<Symbol> midSlot = virtualSlot.getSample();
            List<Symbol> rightSlot = virtualSlot.getSample();

            ////////////////////////////////////////////////////////
            //Pattern matching
            ///////////////////////////////////////////////////////
            
            Dictionary<string, Symbol> patternsMatched = new Dictionary<string, Symbol>();

            //=======STRAIGHT TOP
            if(
                leftSlot[0] == midSlot[0] &&
                 midSlot[0] == rightSlot[0]
            ){
                Symbol symbolMatched = leftSlot[0];
                string patternMatched = "STRAIGHT TOP";
                patternsMatched.Add(patternMatched, symbolMatched);
                PatternMatched(patternMatched);
                SymbolMatched(symbolMatched);
            }
            //=======STRAIGHT MID
            if(
                leftSlot[1] == midSlot[1] &&
                 midSlot[1] == rightSlot[1]
            ){
                Symbol symbolMatched = leftSlot[1];
                string patternMatched = "STRAIGHT MID";
                patternsMatched.Add(patternMatched, symbolMatched);
                PatternMatched(patternMatched);
                SymbolMatched(symbolMatched);
            }

            //=======STRAIGHT BOT
            if(
                leftSlot[2] == midSlot[2] &&
                 midSlot[2] == rightSlot[2]
            ){
                Symbol symbolMatched = leftSlot[2];
                string patternMatched = "STRAIGHT BOT";
                patternsMatched.Add(patternMatched, symbolMatched);
                PatternMatched(patternMatched);
                SymbolMatched(symbolMatched);
            }
        
            //=======DIAG ONE
            if(
                leftSlot[0] == midSlot[1] &&
                 midSlot[1] == rightSlot[2]
            ){
                Symbol symbolMatched = leftSlot[0];
                string patternMatched = "DIAG ONE";
                patternsMatched.Add(patternMatched, symbolMatched);
                PatternMatched(patternMatched);
                SymbolMatched(symbolMatched);
            }

            //=======DIAG TWO
            if(
                leftSlot[2] == midSlot[1] &&
                 midSlot[1] == rightSlot[0]
            ){
                Symbol symbolMatched = leftSlot[2];
                string patternMatched = "DIAG TWO";
                patternsMatched.Add(patternMatched, symbolMatched);
                PatternMatched(patternMatched);
                SymbolMatched(symbolMatched);
            }

            //=======DIAMOND
            if(
                leftSlot[1] == rightSlot[1] &&
                 midSlot[0] == rightSlot[1] &&
                 midSlot[0] == midSlot[2]
            ){
                Symbol symbolMatched = leftSlot[0];
                string patternMatched = "DIAMOND";
                patternsMatched.Add(patternMatched, symbolMatched);
                PatternMatched(patternMatched);
                SymbolMatched(symbolMatched);
            }

            //=======CORNERS
            if(
                leftSlot[0] == rightSlot[0] &&
                leftSlot[2] == rightSlot[2]
            ){
                Symbol symbolMatched = leftSlot[0];
                string patternMatched = "CORNERS";
                patternsMatched.Add(patternMatched, symbolMatched);
                PatternMatched(patternMatched);
                SymbolMatched(symbolMatched);
            }

            //=======STRAIGHT ASCENDING MID
            if(
                leftSlot[1].value == (midSlot[1].value - 1) &&
                midSlot[1].value == (rightSlot[1].value - 1)
            ){
                Symbol symbolMatched = leftSlot[0];
                string patternMatched = "STRAIGHT ASCENDING MID";
                patternsMatched.Add(patternMatched, symbolMatched);
                PatternMatched(patternMatched);
                SymbolMatched(symbolMatched);
            }

            return patternsMatched;
        }

        public float GetRollWinningsIfAny(float stake, Dictionary<string, Symbol> patternsMatched){
            float winnings = 0;

            foreach(var patternWin in patternsMatched.Keys){
                Symbol symbolWin = patternsMatched[patternWin];
                float patternMult = 1f;
                if(patternMatches.ContainsKey(patternWin)){
                    patternMult = patternMatches[patternWin];
                }
                winnings += stake * patternMult * symbolWin.multiplier;
            }

            if(winnings < Balance){
                this.Balance -= winnings;
                return winnings;
            }
            else{
                float toReturn  = this.Balance;
                this.Balance = 0;
                return toReturn;
            }
        
        }

        public void PrintStatisticsToConsole(){
            Console.WriteLine("=====TOTALS=====");
            
            int symbolTotal = 0;
            int patternTotal = 0;

            //get symbol totals
            foreach(var symbol in symbols){
                symbolTotal += getTotalMatchesFromSymbolDict(symbol);
            }
            //get pattern totals
            foreach(var pattern in utilityPatternList){
                patternTotal += getTotalMatchesFromPatternDict(pattern);
            }
            //calculate percentage
            
            foreach(var symbol in symbols){
                var percentageSymbol = ((float)getTotalMatchesFromSymbolDict(symbol)/ symbolTotal) * 100f;
                Console.WriteLine($"Symbol: {symbol.value} : {percentageSymbol}");
            }
            //get pattern totals
            foreach(var pattern in utilityPatternList){
                var percentagePattern = ((float)getTotalMatchesFromPatternDict(pattern)/ patternTotal) * 100f;
                Console.WriteLine($"Pattern: {pattern} : {percentagePattern}");
            }
        }

        private void PatternMatched(string pattern){
            if(patternMatches.ContainsKey(pattern)){
                patternMatches[pattern] = patternMatches[pattern] + 1;
            }
            else{
                patternMatches.Add(pattern, 1);
            }
        }

        private int getTotalMatchesFromPatternDict(string pattern){
            if(patternMatches.ContainsKey(pattern)){
                return patternMatches[pattern];
            }
            else{
                return 0;
            }
        }
        private int getTotalMatchesFromSymbolDict(Symbol symbol){
             if(symbolMatches.ContainsKey(symbol)){
                return symbolMatches[symbol];
            }
            else{
                return 0;
            }
        }
        

        private void SymbolMatched(Symbol symbol){
            if(symbolMatches.ContainsKey(symbol)){
                symbolMatches[symbol] = symbolMatches[symbol] + 1;
            }
            else{
                symbolMatches.Add(symbol, 1);
            }          
        }
    }
}