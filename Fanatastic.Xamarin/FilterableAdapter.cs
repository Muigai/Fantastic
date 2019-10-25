using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Fantastic.Xamarin
{

    public class FilterableAdapter<T> : RecyclerView.Adapter 
    {

        readonly List<T> original;

        readonly Func<ViewGroup, int, (View view, Action<T> updateView)> createView;

        readonly Func<string, IList<T>> filter;

        readonly ConcurrentDictionary<string, IList<T>> filterResults = new ConcurrentDictionary<string, IList<T>>();

        IList<T> filtered;

        public FilterableAdapter(IList<T> original,
                                 Func<ViewGroup, int, (View view, Action<T> updateView)> createView,
                                 Func<string, IList<T>> filter)
        {
            filtered = new List<T>(original);

            this.original = new List<T>(original);

            this.createView = createView;

            this.filter = filter; 
        }

        private int PerformFiltering(string constraint)
        {
            if (string.IsNullOrWhiteSpace(constraint))
            {
                return original.Count;
            }

            var filteredByConstraint = filter(constraint);

            filterResults[constraint] = filteredByConstraint;

            return filteredByConstraint.Count;
        }

        private void PublishResults(string constraint)
        {

            filtered = string.IsNullOrWhiteSpace(constraint) ?
                original :
                filterResults[constraint];

            NotifyDataSetChanged();
        }

        public override int ItemCount => filtered.Count;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position) =>
            ((ViewHolder)holder).UpdateView(filtered[position]);

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType){
            
            var (view, updateView) = createView(parent, viewType);
            
            return new ViewHolder(view, updateView);
        }

        public Filter GetFilter() => new UserFilter(this);

        private class UserFilter : Filter
        {

            readonly FilterableAdapter<T> adapter;

            public UserFilter(FilterableAdapter<T> adapter)
            {
                this.adapter = adapter;
            }

            protected override FilterResults PerformFiltering(Java.Lang.ICharSequence constraint)
            {
                var results = new FilterResults();

                var count = adapter.PerformFiltering(constraint?.ToString());

                results.Count = count;

                return results;
            }

            protected override void PublishResults(Java.Lang.ICharSequence constraint, FilterResults results)
            {
                adapter.PublishResults(constraint?.ToString());
            }
        }

        private class ViewHolder : RecyclerView.ViewHolder
        {

            public ViewHolder(View view, Action<T> updateView) : base(view) {

                UpdateView = updateView;
            }

            internal Action<T> UpdateView { get; }

        }
    }

    public static class FilterableAdapter
    {

        public static FilterableAdapter<T> Make<T>(IList<T> original,
                                                   Func<ViewGroup, int, (View view, Action<T> updateView)> createView,
                                                   Func<string, IList<T>> filter) =>
             new FilterableAdapter<T>(original, createView, filter);
    }

}
